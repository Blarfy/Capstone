<template>
  <div class="form-screen">
    <div class="form-body">
        <h2>{{ title }}</h2>
        <form @submit.prevent="submitForm">
        <div v-for="(label, index) in labels" :key="index">
            <label :for="'input-' + index">{{ label }}:</label>
            <input :id="'input-' + index" v-model="formData[index]" type="text" :required="requiredFields[index]">
        </div>
        <button type="submit">Submit</button>
        </form>
    </div>
  </div>
</template>

<script>
export default {
  props: {
    title: String,
    count: Number,
    labels: Array,
    requiredFields: Array, // Array of booleans indicating whether or not a field is required
  },
  data() {
    return {
      formData: Array(this.count).fill(''), // Initialize form data array with empty strings
    };
  },
  methods: {
    submitForm() {
      this.$emit('form-submitted', this.formData);
    },
  },
};
</script>

<style>
.form-screen {
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

.form-body {
  background-color: #fff;
  padding: 40px;
  border-radius: 5px;
  padding-left: 80px;
  padding-right: 80px;
  box-shadow: 0 0 10px rgba(0, 0, 0, 0.3);
  text-align: center;
}

.form-body h2 {
  margin: 0;
  margin-bottom: 20px;
}

.form-body label {
  display: block;
  margin-bottom: 5px;
}

.form-body input {
  width: 100%;
  padding: 5px;
  margin-bottom: 10px;
  border: 1px solid #ccc;
  border-radius: 3px;
}

.form-body button {
  background-color: #007bff;
  color: #fff;
  padding: 10px 20px;
  border: none;
  border-radius: 3px;
  cursor: pointer;
}

</style>
