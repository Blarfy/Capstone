<template>
    <div>
        <TopBar :is-sidebar-open="isSidebarOpen" :title="barTitle" @search-changed="searchQuery" />

        <div v-if="isFormOpen">
            <DynamicForm :title="formTitle" :count="numberOfFields" :labels="fieldLabels" :required-fields="fieldRequired" @form-submitted="handleFormSubmit" @close-btn="toggleForm" />
        </div>

        <DynamicItemDisplay :is-sidebar-open="isSidebarOpen" :field-names="fieldLabels" :filtered-fields="filteredFields" :hidden-fields="hiddenFields" @delete-item="deletePayment" @share-item="sharePayment" @copied="displayCopyMessage"/>
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
            encryptedPayments: [],
            isFormOpen: false,
            barTitle: 'Payments',
            formTitle: 'Add Payment',
            numberOfFields: 5,
            fieldLabels: ['Name', 'Card Number', 'CVV', 'Expiration Month', 'Expiration Year'],
            fieldRequired: [true, true, true, true, false],
            hiddenFields: [false, true, true, false, false],
        };
    },
    computed: {
        filteredPayments() {
            return this.decryptedPayments.filter((payment) => {
                return payment.plaintextCardName.includes(this.query) || payment.plaintextCardNumber.includes(this.query);
            });
        },
        nameItems() {
            return this.filteredPayments.map((payment) => {
                return payment.plaintextCardName;
            });
        },
        cardNumberItems() {
            return this.filteredPayments.map((payment) => {
                return payment.plaintextCardNumber;
            });
        },
        cvvItems() {
            return this.filteredPayments.map((payment) => {
                return payment.plaintextCardCVV;
            });
        },
        expirationMonthItems() {
            return this.filteredPayments.map((payment) => {
                return payment.plaintextCardExpMonth;
            });
        },
        expirationYearItems() {
            return this.filteredPayments.map((payment) => {
                return payment.plaintextCardExpYear;
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
                this.decryptedPayments = [];
                this.fetchPayments(val.email, val.password, val.key);
            },
            deep: true
        }
    },
    methods: {
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

        // Fetch payments
        async fetchPayments(email, password, key) {
            try {
                this.statusMessage = 'Fetching payments...';
                this.isError = false;
                const response = await fetch('https://localhost:7212/payment?email=' + email + "&password=" + password, { 
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                        'email': email,
                        'password': password,
                        'key': key
                    }
                });

                if (response.status === 200) {
                    const data = await response.json();
                    this.encryptedPayments = data;
                    this.statusMessage = 'Decrypting payments...';

                    // Send encrypted data to decrypt function as a POST request
                    const decryptedData = await fetch('https://localhost:7212/payment/DecryptPayments?key=' + key, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'                        },
                        body: JSON.stringify(data),
                    });

                    if (decryptedData.ok) {
                        this.decryptedPayments = await decryptedData.json();
                        this.statusMessage = '';
                    } else {
                        this.statusMessage = 'Failed to decrypt data. Verify Login Information.';
                        this.isError = true;
                    }
                } else {
                    this.statusMessage = 'Failed to retrieve data. Verify Login Information.';
                    this.isError = true;
                }   
            } catch (error) {
                console.error(`An error occurred: ${error}`);
                this.statusMessage = 'An error occurred. Failed to retrieve data.';
                this.isError = true;
            }
        },

        // Handle form submit
        async handleFormSubmit(formData) {
            try {
                this.formTitle = 'Adding payment...';

                const response = await fetch('https://localhost:7212/payment?email=' + this.userLoginfo.email + '&password=' + this.userLoginfo.password + '&strKey=' + this.userLoginfo.key, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': 'Basic ' + btoa(this.userLoginfo.email + ':' + this.userLoginfo.password), // Check this later
                    },
                    body: JSON.stringify({
                        plaintextCardName: formData[0],
                        plaintextCardNumber: formData[1],
                        plaintextCardCVV: formData[2],
                        plaintextCardExpMonth: formData[3],
                        plaintextCardExpYear: formData[4],
                    }),
                });

                if (response.status === 200) {
                    this.statusMessage = 'Payment added successfully!';
                    this.isError = false;
                    this.toggleForm();
                    this.fetchPayments(this.userLoginfo.email, this.userLoginfo.password, this.userLoginfo.key);
                } else {
                    this.statusMessage = 'Failed to add payment. Verify Login Information.';
                    this.isError = true;
                }

            } catch (error) {
                console.error(`An error occurred: ${error}`);
                this.statusMessage = 'An error occurred. Failed to add payment.';
                this.isError = true;
            }
        },

        async deletePayment(id) {
            try {
                this.statusMessage = 'Deleting payment...';
                this.isError = false;

                const response = await fetch('https://localhost:7212/payment/' + id + '?email=' + this.userLoginfo.email + '&password=' + this.userLoginfo.password + '&strKey=' + this.userLoginfo.key, {
                    method: 'DELETE',
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': 'Basic ' + btoa(this.userLoginfo.email + ':' + this.userLoginfo.password), // Check this later
                    },
                });

                if (response.status === 200) {
                    this.statusMessage = 'Payment deleted successfully!';
                    this.isError = false;
                    this.fetchPayments(this.userLoginfo.email, this.userLoginfo.password, this.userLoginfo.key);
                } else {
                    this.statusMessage = 'Failed to delete payment. Verify Login Information.';
                    this.isError = true;
                }

            } catch (error) {
                console.error(`An error occurred: ${error}`);
                this.statusMessage = 'An error occurred. Failed to delete payment.';
                this.isError = true;
            }
        },

        async sharePayment(index, shareeUsername) {
            this.statusMessage = 'Sharing payment...';

            try {
                const response = await fetch('https://localhost:7212/shared?email=' + this.userLoginfo.email + '&password=' + this.userLoginfo.password + '&type=payment&shareeUsername=' + shareeUsername, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(this.encryptedPayments[index])
                });

                if (response.ok) {
                    this.statusMessage = 'Payment shared successfully!';
                    setTimeout(() => {
                        this.statusMessage = '';
                    }, 1500);                    
                } else {
                    this.statusMessage = 'Failed to share payment. Verify Login Information.';
                    this.isError = true;
                    setTimeout(() => {
                        
                    }, 3000);
                }

            } catch (error) {
                console.error(`An error occurred: ${error}`);
            }
        }
    }
}
</script>

<style>

</style>