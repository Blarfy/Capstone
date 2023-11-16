<template>
  <div>
    <TopBar :title="barTitle" :is-sidebar-open="isSidebarOpen" @search-changed="searchQuery" />

    <div v-if="isFormOpen">
      <DynamicForm :title="formTitle" :count="numberOfFields" :labels="fieldLabels" :required-fields="fieldRequired" @form-submitted="handleFormSubmit" @close-btn="toggleForm" />
    </div>
    
    <div class="content" :class="{ 'open': isSidebarOpen }">
      <div class="column">
        <h2>Location</h2>
        <p v-for="item in appSiteItems" :key="item.id">{{ item.appSite }}</p>
      </div>
      <div class="column">
        <h2>Email</h2>
        <p v-for="item in emailItems" :key="item.id">{{ item.email }}</p>
      </div>
      <div class="column">
        <h2>Username</h2>
        <p v-for="item in usernameItems" :key="item.id">{{ item.username }}</p>
      </div>
      <div class="column">
        <h2>Password</h2>
        <p v-for="item in passwordItems" :key="item.id">{{ item.password }}</p>
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
  name: 'PasswordView',
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
      decryptedPasswords: [],
      isFormOpen: false,
      barTitle: 'Passwords',
      formTitle: 'Add Password',
      numberOfFields: 4,
      fieldLabels: ['Location', 'Email', 'Username', 'Password'],
      fieldRequired: [true, false, false, true],
    };
  },
  computed: {
    filteredPasswords() {
      return this.decryptedPasswords.filter((password) => {
        return password.plaintextLocation.includes(this.query) || password.plaintextEmail.includes(this.query) || password.plaintextUsername.includes(this.query) || password.plaintextIVPass.includes(this.query);
      });
    },
    appSiteItems() {
      return this.filteredPasswords.map((password) => {
        return { id: password.id, appSite: password.plaintextLocation };
      });
    },
    emailItems() {
      return this.filteredPasswords.map((password) => {
        if (password.plaintextEmail === '') {
          return { id: password.id, email: 'ㅤ' };
        }
        else return { id: password.id, email: password.plaintextEmail };
      });
    },
    usernameItems() {
      return this.filteredPasswords.map((password) => {
        if (password.plaintextUsername === '') {
          return { id: password.id, username: 'ㅤ' };
        }
        else return { id: password.id, username: password.plaintextUsername };
      });
    },
    passwordItems() {
      return this.filteredPasswords.map((password) => {
        return { id: password.id, password: password.plaintextIVPass };
      });
    },
  },
  created() {
    if (this.userLoginfo) {
      this.fetchPasswords(this.userLoginfo.email, this.userLoginfo.password, this.userLoginfo.key);
    }
  },
  watch: {
    userLoginfo: {
      handler: function (val) {
        // Update anything upon user login info change
        this.fetchPasswords(val.email, val.password, val.key);
      },
      deep: true
    }
  },
    methods: {
    searchQuery (query) {
      this.query = query;
    },

    async fetchPasswords(email, password, key) {
      try {
        this.statusMessage = 'Loading Data...';
        this.isError = false;

        const response = await fetch('https://localhost:7212/pass?email=' + email + '&password=' + password);

        if (response.ok) {
          const data = await response.json();
          this.statusMessage = 'Decrypting Data...';

          // Send encrypted data to decrypt function as a POST request
          const decryptedData = await fetch('https://localhost:7212/pass/DecryptPasswords?key= ' + key, {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
          });

          if (decryptedData.ok) {
            this.decryptedPasswords = await decryptedData.json();
            this.statusMessage = '';
          } else {
            console.error(`Failed to decrypt data. Status code: ${decryptedData.status}`);
            this.statusMessage = 'Failed to decrypt data. Verify Login Information.';
            this.isError = true;
          }

        } else {
          console.error(`Failed to retrieve data. Status code: ${response.status}`);
          this.statusMessage = 'Failed to retrieve data. Verify Login Information.';
          this.isError = true;
        }
      } catch (error) {
        console.error(`An error occurred: ${error}`);
        this.statusMessage = 'An error occurred. Failed to retrieve data.';
        this.isError = true;
      }
    },
    toggleForm() {
      this.isFormOpen = !this.isFormOpen;
      console.log("toggleForm")
    },
    async handleFormSubmit(formData) {
      // Add Item

      try{
        this.formTitle = 'Adding Password...';
        const response = await fetch('https://localhost:7212/pass?email=' + this.userLoginfo.email + '&password=' + this.userLoginfo.password + "&strKey=" + this.userLoginfo.key, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({
            plaintextLocation: formData[0],
            plaintextEmail: formData[1],
            plaintextUsername: formData[2],
            plaintextIVPass: formData[3]
          })
        });

        if (response.ok) {
          this.isFormOpen = false;
          this.formTitle = 'Add Password';
          this.fetchPasswords(this.userLoginfo.email, this.userLoginfo.password, this.userLoginfo.key);
        } else {
          console.error(`Failed to add password. Status code: ${response.status}`);
          this.formTitle = 'Failed to add password. Are you missing a field?.';
        }
      }
      catch (error) {
        console.error(`An error occurred: ${error}`);
        this.formTitle = 'An error occurred. Failed to add password.';
      }
    },
  },
};
</script>

<style>

</style>

