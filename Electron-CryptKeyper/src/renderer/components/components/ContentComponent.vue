<template>
  <div>
    <div class="top-bar" :class="{ 'open': isSidebarOpen }">
      <div class="title">Passwords</div>
      <div class="search-bar">
        <input type="text" placeholder="Search..." v-model="searchQuery" />
      </div>
    </div>
    <div class="content" :class="{ 'open': isSidebarOpen }">
      <div class="column">
        <h2>App/Site</h2>
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
  </div>
</template>

<script>
export default {
  props: ['isSidebarOpen', 'userLoginfo'],
  data() {
    return {
      searchQuery: '',
      appSiteItems: [],
      emailItems: [],
      usernameItems: [],
      passwordItems: []
    };
  },
  created() {
    if (this.userLoginfo) {
      this.fetchPasswords(this.userLoginfo.email, this.userLoginfo.password, this.userLoginfo.key);
    }
  },
  watch: {
    searchQuery: function (val) {
      // Update anything upon search query change
    },
    userLoginfo: {
      handler: function (val) {
        // Update anything upon user login info change
        this.fetchPasswords(val.email, val.password, val.key);
      },
      deep: true
    }
  },
    methods: {
    async fetchPasswords(email, password, key) {
      try {
        this.appSiteItems.push({ id: 0, appSite: 'Loading Data...' });
        const response = await fetch('https://localhost:7212/pass?email=' + email + '&password=' + password);

        if (response.ok) {
          const data = await response.json();
          this.appSiteItems = [];
          this.encryptedPasswords = data;

          // Send encrypted data to decrypt function as a POST request
          const decryptedData = await fetch('https://localhost:7212/pass/DecryptPasswords?key= ' + key, {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
          });

          console.log(JSON.stringify(data));

          if (decryptedData.ok) {
            const decryptedPasswords = await decryptedData.json();
            this.appSiteItems = [];
            this.emailItems = [];
            this.usernameItems = [];
            this.passwordItems = [];

            // Display the decrypted data
            for (let i = 0; i < decryptedPasswords.length; i++) {
              this.appSiteItems.push({ id: i, appSite: decryptedPasswords[i].plaintextLocation });
              this.emailItems.push({ id: i, email: decryptedPasswords[i].plaintextEmail });
              this.usernameItems.push({ id: i, username: decryptedPasswords[i].plaintextUsername });
              this.passwordItems.push({ id: i, password: decryptedPasswords[i].plaintextIVPass });
            }
          } else {
            console.error(`Failed to decrypt data. Status code: ${decryptedData.status}`);
            this.appSiteItems.push({ id: 1, appSite: 'Failed to decrypt data. Verify Login Information.' });
          }

        } else {
          console.error(`Failed to retrieve data. Status code: ${response.status}`);
          this.appSiteItems.push({ id: 0, appSite: 'Failed to retrieve data. Verify Login Information.' });
        }
      } catch (error) {
        console.error(`An error occurred: ${error}`);
        this.appSiteItems.push({ id: 2, appSite: 'An error occurred. Failed to retrieve data.' });
      }
    }
},

};
</script>


<style scoped>
.top-bar {
  margin-left: 50px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 15px;
  margin-right: 0;
  background-color: #fff;
  border-bottom: 2px solid black; /* Fix the border declaration */
  color: black; /* Text color */
  transition: margin-left 0.3s;
}

.top-bar.open {
  margin-left: 200px;
}

.title {
  font-size: 24px;
  font-weight: bold;
}

.search-bar {
  flex: 1;
  text-align: right;
}

.search-bar input {
  padding: 5px;
  width: 23%; 
  border: 1px solid #ccc;
}

.content {
  margin-left: 50px;
  transition: margin-left 0.3s;
  display: flex;
}

.content.open {
  margin-left: 200px;
}

.column {
  margin-top: 20px;
  padding: 15px;
  width: 25%;
  border: 1px solid #ccc;
  flex: 1;
}

.column h2 {
  font-weight: bold;
}

/* Adjust as needed for responsiveness */
@media (max-width: 768px) {
  .column {
    flex: 0 0 100%; /* On smaller screens, make each column take up 100% of the width */
  }
}
</style>

