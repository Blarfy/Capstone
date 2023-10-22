<template>
  <div v-if="!isLoggedIn" class="login-screen">
    <div class="login-form">
      <h2>Login</h2>
      <form @submit.prevent="login">
        <label for="username">Username:</label>
        <input type="text" id="username" v-model="username" required>
        <label for="password">Password:</label>
        <input type="password" id="password" v-model="password" required>
        <button type="submit">Log In</button>
      </form>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      username: '',
      password: '',
      isLoggedIn: true, //set to false later
    };
  },
  methods: {
    async login() {
      try {
        const response = await api.post('/api/Login', {
          email: this.username,
          password: this.password,
        });

        const key = response.data; // Assuming your API returns the lockbox key
        if (key) {
          // User is authenticated; set isLoggedIn to true
          this.isLoggedIn = true;
          // Fetch and decrypt passwords here
          this.fetchAndDecryptPasswords(key);
        } else {
          // Handle authentication failure
        }
      } catch (error) {
        // Handle the API request error
      }
    },
  },

};
</script>

<style>
.login-screen {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.7);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 999;
}

.login-form {
  background-color: #fff;
  padding: 20px;
  border-radius: 5px;
  box-shadow: 0 0 10px rgba(0, 0, 0, 0.3);
  text-align: center;
}

.login-form h2 {
  margin: 0;
  margin-bottom: 20px;
}

.login-form label {
  display: block;
  margin-bottom: 5px;
}

.login-form input {
  width: 100%;
  padding: 5px;
  margin-bottom: 10px;
  border: 1px solid #ccc;
  border-radius: 3px;
}

.login-form button {
  background-color: #007bff;
  color: #fff;
  padding: 10px 20px;
  border: none;
  border-radius: 3px;
  cursor: pointer;
}
</style>
