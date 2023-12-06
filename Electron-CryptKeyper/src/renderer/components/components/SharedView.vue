<template>
    <div>
        <TopBar :title="barTitle" :is-sidebar-open="isSidebarOpen" @search-changed="searchQuery"></TopBar>

        <ul class="list" style="list-style-type: none;" :class="{'aside': isSidebarOpen}">
            <li class="shared-li" v-for="(item, index) in filteredShared" :key="index">
                <div class="item-container" @click="togglePopup(index)">
                    <h2>{{ item }}</h2>
                    <div class="field">{{ decryptedShared[index].itemType[0].toUpperCase() + decryptedShared[index].itemType.slice(1) }}</div>
                </div>
            </li>
        </ul>

        <StatusBlob :message="statusMessage" :is-error="isError"></StatusBlob>

        <InfoPopup :chosenItem="decryptedShared[selectedItem]" :is-open="isInfoOpen" @close-info-popup="togglePopup" @copied="displayCopyMessage"></InfoPopup>

        <!-- <button id="info-btn" @click="togglePopup">?</button> -->

    </div>
</template>

<script>
import TopBar from './TopBar.vue';
import StatusBlob from './StatusBlob.vue';
import InfoPopup from './InfoPopup.vue';

export default {
    name: 'SharedView',
    components: {
    TopBar,
    StatusBlob,
    InfoPopup
},
    props: ['isSidebarOpen', 'userLoginfo'],
    data() {
        return {
            query: '',
            statusMessage: '',
            isError: false,
            decryptedShared: [],
            barTitle: 'Shared',
            isInfoOpen: false,
            selectedItem: -1,
        };
    },
    computed: {
        filteredShared() {
            let filtered = [];
            for (let i = 0; i < this.decryptedShared.length; i++) {
                switch (this.decryptedShared[i].itemType) {
                    case 'password':
                        let newTitle = this.decryptedShared[i].item.plaintextLocation;
                        filtered.push(newTitle);
                        break;
                    case 'payment':
                        let newTitlePay = this.decryptedShared[i].item.plaintextCardName;
                        filtered.push(newTitlePay);
                        break;
                    case 'note':
                        let newTitleNote = this.decryptedShared[i].item.plaintextTitle;
                        filtered.push(newTitleNote);
                        break;
                    default:
                        break;
                }
            }
            return filtered;
        },
    },
    created() {
        if (this.userLoginfo) {
            this.fetchShared(this.userLoginfo.email, this.userLoginfo.password, this.userLoginfo.key);
        }
    },
    watch: {
        userLoginfo: {
            handler() {
                this.decryptedShared = [];
                this.fetchShared(this.userLoginfo.email, this.userLoginfo.password, this.userLoginfo.key);
            },
            deep: true
        }
    },
    methods: {
        async fetchShared(email, password) {
            try {
                this.statusMessage = 'Fetching shared items...';
                this.isError = false;

                const response = await fetch('https://localhost:7212/shared?email=' + email + '&password=' + password);
                if(response.ok) {
                    const data = await response.json();
                    this.statusMessage = 'Decrypted shared items';

                    const decryptedData = await fetch('https://localhost:7212/shared/DecryptShared?key=' + this.userLoginfo.key, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify(data),
                    });

                    if (decryptedData.ok) {
                        this.decryptedShared = await decryptedData.json();
                        this.statusMessage = '';
                    } else {
                        this.statusMessage = 'Error fetching shared items';
                        this.isError = true;
                        setTimeout(() => {
                            this.statusMessage = '';
                            this.isError = false;
                        }, 1500);
                    }
                } else {
                    this.statusMessage = 'Error fetching shared items';
                    this.isError = true;
                    setTimeout(() => {
                        this.statusMessage = '';
                        this.isError = false;
                    }, 1500);
                }
                

                
            } catch (error) {
                this.statusMessage = 'Error fetching shared items';
                this.isError = true;
                setTimeout(() => {
                    this.statusMessage = '';
                    this.isError = false;
                }, 1500);
            }
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
        },
        togglePopup() {
            this.isInfoOpen = !this.isInfoOpen;
        },
        togglePopup(index) {
            this.isInfoOpen = !this.isInfoOpen;
            this.selectedItem = index;
        },
    }
}

</script>

<style>
#info-btn {
    position: fixed;
    top: 0;
    right: 0;
    width: 50px;
    height: 50px;
    border-radius: 50%;
    background-color: #fff;
    border: 1px solid #000;
    font-size: 1.5rem;
    font-weight: bold;
    cursor: pointer;
    z-index: 999;
}

.item-container {
    display: flex;
    flex-direction: column;
    align-items: left;
    padding: 20px;
    cursor: pointer;
    width: auto;
    border-bottom: 2px solid #c1c1c1;
    border-top: 2px solid #c1c1c1;
    transition: background-color 0.3s;
}

.shared-li {
    margin: 0px;
    width: 100%;
    transition: background-color 0.3s;
}

.shared-li :hover {
    background-color: #c1c1c1;
    transition: background-color 0.3s;
}

.field {
    font-size: 1.2rem;
    font-weight: bold;
    color: slategray;
}

.list {
    margin-left: 10px;
    margin-top: 0px;
    transition: margin-left 0.3s;
    display: flex;
    flex-direction: column;
}

.list.aside {
    margin-left: 160px;
}

</style>