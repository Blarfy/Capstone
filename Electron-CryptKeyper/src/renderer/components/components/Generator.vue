<template>
  <div id="password-generator">
    <h2>Field Generator</h2>
    <form>
      <label for="length">Password Length: {{ passwordLength }}</label>
      <input type="range" v-model="passwordLength" min="12" max="32">

      <label><input type="checkbox" v-model="includeLetters"> Include Letters (A-Z, a-z)</label>
      <label><input type="checkbox" v-model="includeNumbers"> Include Numbers (0-9)</label>
      <label><input type="checkbox" v-model="includeSymbols"> Include Symbols (!@#$%^&*)</label>

    </form>

    <div id="password-output" @click="copyGenerated">{{ generatedPassword }}</div>
    <button id="copy-button" @click="copyGenerated">Copy</button>
  </div>
</template>

<script>
export default {
  data() {
    return {
      passwordLength: 12,
      includeLetters: true,
      includeNumbers: true,
      includeSymbols: true,
      generatedPassword: ''
    };
  },
  created() {
    this.generatePassword();
  },
  watch: {
    passwordLength() {
      this.generatePassword();
    },
    includeLetters() {
      this.generatePassword();
    },
    includeNumbers() {
      this.generatePassword();
    },
    includeSymbols() {
      this.generatePassword();
    }
  },
  methods: {
    async generatePassword() {
      const letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
      const numbers = "0123456789";
      const symbols = "!@#$%^&*";

      let charset = '';
      if (this.includeLetters) {
        charset += letters;
      }
      if (this.includeNumbers) {
        charset += numbers;
      }
      if (this.includeSymbols) {
        charset += symbols;
      }


      for (let i = 0; i < Math.floor(Math.random() * 3) + 4; i++) {

        let password = '';
        for (let i = 0; i < this.passwordLength; i++) {
            const randomIndex = Math.floor(Math.random() * charset.length);
            password += charset.charAt(randomIndex);
        }

        this.generatedPassword = password;

        await new Promise(resolve => setTimeout(resolve, 50 ));
      }
    },
    copyGenerated() {
        try {
            if(!navigator.clipboard) throw('Unsupported browser');
            navigator.clipboard.writeText(this.generatedPassword);
        } catch(error) {
            // Fallback for insecure browsers / Localhost testing
            const textArea = document.createElement('textarea');
            textArea.value = this.generatedPassword;
            document.body.appendChild(textArea);
            textArea.focus();
            textArea.select();
            document.execCommand('copy');
            document.body.removeChild(textArea);
        }

        this.$emit('copied')
    }
  }
};
</script>

<style scoped>
#password-generator {
  background-color: #fff;
  padding: 40px;
  padding-bottom: 10px;
  border-radius: 5px;
  box-shadow: 0 0 10px rgba(0, 0, 0, 0.3);
  text-align: center;
  margin: 20px;
  position: absolute;
  left: 15%;
  width: 15%;
}

form {
  margin-top: 20px;
  border-bottom: 2px solid darkgray;
}

label {
  display: block;
  margin: 10px 0;
}

input[type="range"] {
  width: 80%;
  margin-top: 5px;
}

#password-output {
  overflow: visible;
  margin: 20px -30%;
  font-weight: bold;
}

#copy-button {
  background-color: #007bff;
  color: #fff;
  padding: 10px 20px;
  border: none;
  border-radius: 3px;
  cursor: pointer;
}
</style>
