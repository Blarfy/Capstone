<template>
  <div v-if="!isRegistered" class="login-screen">
    <div class="register-form">
      <h2>Register</h2>
      <form @submit.prevent="register">
        <label for="username">Username:</label>
        <input type="text" id="username" v-model="username" required>
        <label for="password">Master Password:</label>
        <input type="password" id="password" v-model="password" required>
        <label for="confirmPassword">Confirm Master Password:</label>
        <input type="password" id="confirmPassword" v-model="confirmPassword" required>
        <label for="error" style="color: red;">{{ error }}</label>
        <button type="submit"> {{ registerBtnTxt }}</button>
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
      confirmPassword: '',
      error: '',
      isRegistered: false,
      registerBtnTxt: 'Register'
    };
  },
  methods: {
    async register() {
      this.error = '';
      if (this.password !== this.confirmPassword) {
        this.error = 'Passwords do not match';
        return;
      }

      try {
        this.registerBtnTxt = 'Registering...';

        const response = await fetch('https://localhost:7212/account?email=' + this.username + '&masterPass=' + this.password, {method: 'POST'});
        
        if(response.ok) { 
            this.isRegistered = true;
            this.$emit('registered');
        } else {
            throw new Error('Error registering account');
        }
      } catch(error) {
        this.error = 'Error registering account';
        this.registerBtnTxt = 'Register';
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

.register-form {
  background-color: #fff;
  padding: 50px;
  border-radius: 5px;
  box-shadow: 0 0 10px rgba(0, 0, 0, 0.3);
  text-align: center;
}

.register-form h2 {
  margin: 0;
  margin-bottom: 20px;
}

.register-form label {
  display: block;
  margin-bottom: 5px;
}

.register-form input {
  width: 100%;
  padding: 5px;
  margin-bottom: 10px;
  border: 1px solid #ccc;
  border-radius: 3px;
}

.register-form button {
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
  cursor: pointer;
}

.register:hover {
  color: #007bff;
}
</style>