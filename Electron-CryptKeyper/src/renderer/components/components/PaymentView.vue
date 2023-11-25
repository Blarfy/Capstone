<template>
    <div>
        <TopBar :is-sidebar-open="isSidebarOpen" :title="barTitle" @search-changed="searchQuery" />

        <div v-if="isFormOpen">
            <DynamicForm :title="formTitle" :count="numberOfFields" :labels="fieldLabels" :required-fields="fieldRequired" @form-submitted="handleFormSubmit" @close-btn="toggleForm" />
        </div>

        <!-- <div class="content" :class="{ 'open': isSidebarOpen}">
            <div class="column">
                <h2>Name</h2>
                <p v-for="item in nameItems" :key="item.id">{{ item.name }}</p>
            </div>
            <div class="column">
                <h2>Card Number</h2>
                <p v-for="item in cardNumberItems" :key="item.id">{{ item.cardNumber }}</p>
            </div>
            <div class="column">
                <h2>CVV</h2>
                <p v-for="item in cvvItems" :key="item.id">{{ item.cvv }}</p>
            </div>
            <div class="column">
                <h2>Expiration Month</h2>
                <p v-for="item in expirationMonthItems" :key="item.id">{{ item.expirationMonth }}</p>
            </div>
            <div class="column">
                <h2>Expiration Year</h2>
                <p v-for="item in expirationYearItems" :key="item.id">{{ item.expirationYear }}</p>
            </div>
        </div> -->

        <DynamicItemDisplay :is-sidebar-open="isSidebarOpen" :field-names="fieldLabels" :filtered-fields="filteredFields"/>
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
    name: 'PaymentView',
    components: {
        DynamicForm, // Replace with payment form later
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
            decryptedPayments: [],
            isFormOpen: false,
            barTitle: 'Payments',
            formTitle: 'Add Payment',
            numberOfFields: 5,
            fieldLabels: ['Name', 'Card Number', 'CVV', 'Expiration Month', 'Expiration Year'],
            fieldRequired: [true, true, true, true, false],
        };
    },
    computed: {
        filteredPayments() {
            return this.decryptedPayments.filter((payment) => {
                return payment.plaintextName.includes(this.query) || payment.plaintextCardNumber.includes(this.query);
            });
        },
        nameItems() {
            return this.filteredPayments.map((payment) => {
                return {
                    id: payment.id,
                    name: payment.plaintextName,
                };
            });
        },
        cardNumberItems() {
            return this.filteredPayments.map((payment) => {
                return {
                    id: payment.id,
                    cardNumber: payment.plaintextCardNumber,
                };
            });
        },
        cvvItems() {
            return this.filteredPayments.map((payment) => {
                return {
                    id: payment.id,
                    cvv: payment.plaintextCVV,
                };
            });
        },
        expirationMonthItems() {
            return this.filteredPayments.map((payment) => {
                return {
                    id: payment.id,
                    expirationMonth: payment.plaintextExpirationMonth,
                };
            });
        },
        expirationYearItems() {
            return this.filteredPayments.map((payment) => {
                return {
                    id: payment.id,
                    expirationYear: payment.plaintextExpirationYear,
                };
            });
        },
        filteredFields() {
            return [
                this.nameItems,
                this.cardNumberItems,
                this.cvvItems,
                this.expirationMonthItems,
                this.expirationYearItems
            ]
        }
    },
    created() {
        if (this.userLoginfo) {
            this.fetchPayments(this.userLoginfo.email, this.userLoginfo.password, this.userLoginfo.key);
        }
    },
    watch: {
        userLoginfo: {
            handler: function (val) {
                this.fetchPayments(val.email, val.password, val.key);
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

        // Fetch notes
        async fetchPayments(email, password, key) {
            try {
                this.statusMessage = 'Fetching payments...';
                this.isError = false;
                const response = await fetch('http://localhost:7212/payments', {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                        'email': email,
                        'password': password,
                        'key': key
                    }
                });
            } catch (error) {
                console.error(`An error occurred: ${error}`);
                this.statusMessage = 'An error occurred. Failed to retrieve data.';
                this.isError = true;
            }
        },

        // Handle form submit
    }
}
</script>

<style>

</style>