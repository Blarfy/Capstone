<template>
    <div>
        <TopBar :title="barTitle" :is-sidebar-open="isSidebarOpen" @search-changed="searchQuery"></TopBar>

        <StatusBlob :message="statusMessage" :is-error="isError"></StatusBlob>

    </div>
</template>

<script>
import TopBar from './TopBar.vue';
import StatusBlob from './StatusBlob.vue';

export default {
    name: 'SharedView',
    components: {
        TopBar,
        StatusBlob
    },
    props: ['isSidebarOpen', 'userLoginfo'],
    data() {
        return {
            query: '',
            statusMessage: ';)',
            isError: false,
            decryptedShared: [],
            barTitle: 'Shared',
        };
    },
    computed: {
        filteredShared() {
            return this.decryptedShared.filter((shared) => {
                return shared.plaintextLocation.includes(this.query) || shared.plaintextEmail.includes(this.query) || shared.plaintextUsername.includes(this.query) || shared.plaintextIVPass.includes(this.query);
            });
        },
        appSiteItems() {
            return this.filteredShared.map((shared) => {
                return shared.plaintextLocation;
            });
        },
        emailItems() {
            return this.filteredShared.map((shared) => {
                if (shared.plaintextEmail === '') {
                    return 'ㅤ';
                }
                else return shared.plaintextEmail;
            });
        },
        usernameItems() {
            return this.filteredShared.map((shared) => {
                if (shared.plaintextUsername === '') {
                    return 'ㅤ';
                }
                else return shared.plaintextUsername;
            });
        },
        passwordItems() {
            return this.filteredShared.map((shared) => {
                return shared.plaintextIVPass;
            });
        },
        filteredFields() {
            return [this.appSiteItems, this.emailItems, this.usernameItems, this.passwordItems]
        }
    },
    created() {
        if (this.userLoginfo) {
            this.fetchShared(this.userLoginfo.email, this.userLoginfo.password, this.userLoginfo.key);
        }
    },
    watch: {
        userLoginfo: {
            handler() {
                this.fetchShared(this.userLoginfo.email, this.userLoginfo.password, this.userLoginfo.key);
            },
            deep: true
        }
    },
    methods: {
        async fetchShared(email, password, key) {
            // fetch shared items
        },
        searchQuery(query) {
            this.query = query;
        },
        displayCopyMessage() {
            this.statusMessage = 'Copied to clipboard!';
            setTimeout(() => {
                this.statusMessage = '';
            }, 1500);
        },
        toggleForm() {
            this.isFormOpen = !this.isFormOpen;
        }
    }
}

</script>