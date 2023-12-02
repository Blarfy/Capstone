<template>
  <div class="form-screen">
    <div class="form-body">
        <a class="close-btn" @click="closeBtn">&times;</a>
        <h2>{{ title }}</h2>
        <form @submit.prevent="submitForm">
        <div v-for="(label, index) in labels" :key="index">
            <label :for="'input-' + index">{{ label }}:</label>
            <input :id="'input-' + index" v-model="formData[index]" type="text" :required="requiredFields[index]">
        </div>
        <button type="submit">Submit</button>
        </form>
    </div>
    <Generator v-if="isGenerator" @copied="$emit('copied')"/>
  </div>
</template>

<script>
import Generator from './Generator.vue';

export default {
  name: 'DynamicForm',
  components: {
    Generator,
  },
  props: {
    title: String,
    count: Number,
    labels: Array,
    requiredFields: Array, // Array of booleans indicating whether or not a field is required
    isGenerator: Boolean,
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
    closeBtn() {
      this.$emit('close-btn');
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
  padding: 10px;
  border-radius: 5px;
  padding-left: 80px;
  padding-right: 80px;
  box-shadow: 0 0 10px rgba(0, 0, 0, 0.3);
  text-align: center;
}

.form-body h2 {
  margin: 0;
  margin-bottom: 0px;
  padding: 30px;
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

.close-btn {
  margin-top: 10px;
  margin-right: -150%;
  font-size: 20px;
  background: none;
  border: none;
  cursor: pointer;
  color: #ccc;
}

</style>
