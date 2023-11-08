<template>
  <div class="bottom-center-message">
    <div class="message-box" v-if="message" :class="{'is-error': isError}">{{ message }}</div>
  </div>
</template>

<script>
export default {
  props: {
    message: String,
    isError: Boolean,
  },
};
</script>

<style scoped>
.bottom-center-message {
  position: absolute;
  bottom: 50px;
  left: 50%;
  transform: translateX(-50%);
  z-index: 999; /* Ensures it displays above other components */
  text-align: center;
}

.message-box {
  background-color: #c2c2c2;
  padding: 20px 40px;
  border-radius: 5px;
}

.is-error {
  color: #ff0000;
}
</style>
