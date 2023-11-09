<template>
    <div>
        <TopBar :is-sidebar-open="isSidebarOpen" :title="barTitle" @search-changed="searchQuery" />

        <div v-if="isFormOpen">
            <DynamicForm :title="formTitle" :count="numberOfFields" :labels="fieldLabels" :required-fields="fieldRequired" @formSubmitted="handleFormSubmit" />
        </div>

        <div class="content" :class="{ 'open': isSidebarOpen }">
            <div class="column">
                <h2>Title</h2>
                <p v-for="item in titleItems" :key="item.id">{{ item.title }}</p>
            </div>
            <div class="column">
                <h2>Note</h2>
                <p v-for="item in noteItems" :key="item.id">{{ item.note }}</p>
            </div>
        </div>
        <StatusBlob :message="statusMessage" :is-error="isError" />
        <button class="plus-button" @click="toggleForm">+</button>
    </div>
</template>

<script>
import DynamicForm from './DynamicForm.vue';
import TopBar from './TopBar.vue';
import StatusBlob from './StatusBlob.vue';
export default {
    name: 'NotesView',
    components: {
        DynamicForm,
        TopBar,
        StatusBlob
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
        };
    },
    computed: {
        filteredNotes() {
            return this.decryptedNotes.filter((note) => {
                return note.title.toLowerCase().includes(this.query.toLowerCase()) || note.note.toLowerCase().includes(this.query.toLowerCase());
            });
        },
        titleItems() {
            return this.filteredNotes.map((note) => {
                return {
                    id: note.id,
                    title: note.title,
                };
            });
        },
        noteItems() {
            return this.filteredNotes.map((note) => {
                return {
                    id: note.id,
                    note: note.note,
                };
            });
        },
    },
    created() {
        if (this.userLoginfo) {
            this.fetchNotes(this.userLoginfo.email, this.userLoginfo.password, this.userLoginfo.key);
        }
    },
    methods: {
        searchQuery(query) {
            this.query = query;
        },

        toggleForm() {
            this.isFormOpen = !this.isFormOpen;
        },

        async fetchNotes(email, password, key) {
            try {
                this.statusMessage = 'Fetching notes...';
                this.isError = false;

                const response = await fetch('http://localhost:7212/notes?email=' + email + '&password=' + password, {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': 'Basic ' + btoa(email + ':' + password), // Check this later
                    },
                });

                if (response.status === 200) {
                    const data = await response.json();
                    this.statusMessage = '';

                    // Send encrypted data to decrypt function as a POST request
                    const decryptedData = await fetch('http://localhost:7212/notes/DecryptNotes?key=' + key, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'Authorization': 'Basic ' + btoa(email + ':' + password), // Check this later
                        },
                        body: JSON.stringify(data),
                    });

                    if (decryptedData.status === 200) {
                        this.decryptedNotes = await decryptedData.json();
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
                const response = await fetch('http://localhost:7212/notes?email=' + this.userLoginfo.email + '&password=' + this.userLoginfo.password + '&strKey=' + this.userLoginfo.key, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': 'Basic ' + btoa(this.userLoginfo.email + ':' + this.userLoginfo.password), // Check this later
                    },
                    body: JSON.stringify({
                        title: formData[0],
                        note: formData[1],
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