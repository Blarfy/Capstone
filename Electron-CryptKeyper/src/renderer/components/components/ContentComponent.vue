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
  props: ['isSidebarOpen'],
  data() {
    return {
      searchQuery: '',
      appSiteItems: [
        { id: 1, appSite: 'Example App 1' },
        { id: 2, appSite: 'Sample Website 1' },
        { id: 3, appSite: 'Test App 2' },
      ],
      usernameItems: [
        { id: 1, username: 'user1' },
        { id: 2, username: 'john_doe' },
        { id: 3, username: 'test_user' },
      ],
      passwordItems: [
        { id: 1, password: 'password123' },
        { id: 2, password: 'securePass!2023' },
        { id: 3, password: 'testing456' },
      ]
    };
  },
  created() {
    // fetch the data when the view is created and the data is
    // already being observed
    this.fetchPasswords();
  },
  watch: {
      // call again the method if the route changes
      '$route': 'fetchPasswords'
  },
    methods: {
    async fetchPasswords() {
      try {
        const response = await fetch('https://localhost:7212/pass?email=Gweppy&password=password');

        if (response.ok) {
          console.log('Data retrieved successfully!')
          console.log(response)
          const data = await response.json();
          console.log(data)
          this.encryptedPasswords = data;
        } else {
          console.error(`Failed to retrieve data. Status code: ${response.status}`);
        }
      } catch (error) {
        console.error(`An error occurred: ${error}`);
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
  background-color: #fff; /* Replace with your desired background color */
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
  display: flex; /* Add display: flex to the content container */
}

.content.open {
  margin-left: 200px;
}

.column {
  margin-top: 20px;
  padding: 15px;
  width: 25%; /* Adjust the width as needed, but for side-by-side columns, the sum should not exceed 100% */
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

