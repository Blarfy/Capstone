<template>
    <div>
        <TopBar :is-sidebar-open="isSidebarOpen" :title="barTitle" @search-changed="searchQuery" />

        <div v-if="isFormOpen">
            <DynamicForm :title="formTitle" :count="numberOfFields" :labels="fieldLabels" :required-fields="fieldRequired" @form-submitted="handleFormSubmit" @close-btn="toggleForm" />
        </div>

        <DynamicItemDisplay :is-sidebar-open="isSidebarOpen" :field-names="fieldLabels" :filtered-fields="filteredFields" :hidden-fields="hiddenFields" @copied="displayCopyMessage" />

        <StatusBlob :message="statusMessage" :is-error="isError" />
        <button class="plus-button" @click="toggleForm">+</button>
    </div>
</template>

<script>
import DynamicForm from './DynamicForm.vue';
import TopBar from './TopBar.vue';
import StatusBlob from './StatusBlob.vue';
import DynamicItemDisplay from './DynamicItemDisplay.vue';

export default {
    name: 'NotesView',
    components: {
        DynamicForm,
        TopBar,
        StatusBlob,
        DynamicItemDisplay
    },
    props: ['isSidebarOpen', 'userLoginfo'],
    data() {
        return {
            query: '',
            statusMessage: '',
            isError: false,
            decryptedNotes: [],
            isFormOpen: false,
            barTitle: 'Notes',
            formTitle: 'Add Note',
            numberOfFields: 2,
            fieldLabels: ['Title', 'Note'],
            fieldRequired: [true, true],
            hiddenFields: [false, false],
        };
    },
    computed: {
        filteredNotes() {
            return this.decryptedNotes.filter((note) => {
                return note.plaintextTitle.includes(this.query) || note.plaintextNote.includes(this.query);
            });
        },
        titleItems() {
            return this.filteredNotes.map((note) => {
                return note.plaintextTitle;
            });
        },
        noteItems() {
            return this.filteredNotes.map((note) => {
                return note.plaintextNote;
            });
        },
        filteredFields() {
            return [this.titleItems, this.noteItems]
        }
    },
    created() {
        if (this.userLoginfo) {
            this.fetchNotes(this.userLoginfo.email, this.userLoginfo.password, this.userLoginfo.key);
        }
    },
    watch: {
        userLoginfo: {
            handler: function (val) {
                this.fetchNotes(val.email, val.password, val.key);
            },
            deep: true
        }
    },
    methods: {
        searchQuery(query) {
            this.query = query;
        },

        toggleForm() {
            this.isFormOpen = !this.isFormOpen;
        },

        displayCopyMessage() {
            this.statusMessage = 'Copied to clipboard!';
            setTimeout(() => {
                this.statusMessage = '';
            }, 1500);
        },

        async fetchNotes(email, password, key) {
            try {
                this.statusMessage = 'Fetching notes...';
                this.isError = false;

                const response = await fetch('https://localhost:7212/note?email=' + email + '&password=' + password, {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': 'Basic ' + btoa(email + ':' + password), // Check this later
                    },
                });

                if (response.status === 200) {
                    const data = await response.json();
                    this.statusMessage = 'Decrypting notes...';

                    // Send encrypted data to decrypt function as a POST request
                    const decryptedData = await fetch('https://localhost:7212/note/DecryptNotes?key=' + key, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'Authorization': 'Basic ' + btoa(email + ':' + password), // Check this later
                        },
                        body: JSON.stringify(data),
                    });

                    if (decryptedData.ok) {
                        this.decryptedNotes = await decryptedData.json();
                        this.statusMessage = '';
                    } else {
                        this.statusMessage = 'Failed to decrypt data. Verify Login Information.';
                        this.isError = true;
                    }
                } else {
                    this.statusMessage = 'Failed to retrieve data. Verify Login Information.';
                    this.isError = true;
                }
            } catch (err) {
                this.statusMessage = 'Error fetching notes';
                this.isError = true;
            }
        },

        async handleFormSubmit(formData) {
            // Add Item
            try {
                this.formTitle = 'Adding Note...';
                const response = await fetch('https://localhost:7212/note?email=' + this.userLoginfo.email + '&password=' + this.userLoginfo.password + '&strKey=' + this.userLoginfo.key, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': 'Basic ' + btoa(this.userLoginfo.email + ':' + this.userLoginfo.password), // Check this later
                    },
                    body: JSON.stringify({
                        plaintextTitle: formData[0],
                        plaintextNote: formData[1],
                    }),
                });

                if (response.status === 200) {
                    this.isFormOpen = false;
                    this.formTitle = 'Add Note';
                    this.fetchNotes(this.userLoginfo.email, this.userLoginfo.password, this.userLoginfo.key);
                } else {
                    this.formTitle = 'Failed to add note. Is a note with that title already present?';
                }
            } catch (err) {
                this.statusMessage = 'Error adding note';
                this.isError = true;
            }
        },
    }
};
</script>

<style>

</style>