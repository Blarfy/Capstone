<template>
  <div v-if="!isLoggedIn" class="login-screen">
    <div class="login-form">
      <h2>Login</h2>
      <form @submit.prevent="login">
        <label for="username">Username:</label>
        <input type="text" id="username" v-model="username" required>
        <label for="password">Password:</label>
        <input type="password" id="password" v-model="password" required>
        <label for="error" style="color: red;">{{ error }}</label>
        <button type="submit"> {{ loginBtnTxt }}</button>
      </form>
      <a class="register" @click="toggleRegister">Register</a>
    </div>
  </div>
  <Register v-else-if="!isRegistered" @registered="toggleRegister"/>
</template>

<script>
import Register from './Register.vue';

export default {
  name: 'Login',
  components: {
    Register
  },
  data() {
    return {
      username: 'Gweppy',
      password: 'password',
      error: '',
      isLoggedIn: false,
      isRegistered: true,
      loginBtnTxt: 'Login'
    };
  },
  methods: {
    toggleRegister() {
      this.isLoggedIn = !this.isLoggedIn;
      this.isRegistered = !this.isRegistered;
    },
    async login() {
      this.loginBtnTxt = 'Logging in...';
      this.error = '';
      try {
        const response = await fetch('https://localhost:7212/account?email=' + this.username + '&password=' + this.password);

        let key = await response.text();
        if (key) {
          // Emit login info to parent component
          this.$emit('changeView', 'password')
          this.$emit('userLogin', this.username, this.password, key);

          // User is authenticated; set isLoggedIn to true
          this.isLoggedIn = true;
        } else {
          // Handle authentication failure
          this.error = 'Invalid username or password';
          this.loginBtnTxt = 'Login';
        }
      } catch (error) {
        // Handle the API request error
        console.log(error);
        this.error = 'Invalid username or password'
        this.loginBtnTxt = 'Login';
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
  padding: 50px;
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
  margin-bottom: 10%;
}

.register {
  color: lightgray;
  text-decoration: underline;
  text-align: center; 
}
</style>
