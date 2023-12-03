<template>
<div class="info-screen" v-if="isOpen">
    <div class="info-content" :class="{'open': isOpenClass}">
        <section style="padding: 20px;">
            <!-- Data goes here -->
            <div>{{ data.item }}</div>
        </section>  
    </div>
    <div class="info-clickbox" @click="closeInfoPopup"></div> 
  </div>
</template>

<script>
export default {
  data() {
    return {
        isOpenClass: false,
    };
  },
  methods: {
    closeInfoPopup() {
      this.$emit('close-info-popup');
    },
  },
    props: {
        isOpen: Boolean,
        data: Object,
        type: String,
    },
    watch: {
        async isOpen() {
            // Wait .1 seconds for isOpen to be set to true before setting isOpenClass to true
            await new Promise((resolve) => setTimeout(resolve, 100));
            this.isOpenClass = this.isOpen;
        },
        data() {
            // Change how data is displayed here
        },
    },
};
</script>

<style scoped>
.info-screen {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.7);
  display: flex;
  justify-content: right;
  align-items: right;
  z-index: 20;
}

.info-clickbox {
  width: 60%;
  height: 100%;
  position: fixed;
  left: 0;
  margin-left: auto;
}

.info-content {
  width: 0%;
  height: 100%;
  position: fixed;
  right: 0;
  margin-left: auto;
  background-color: aliceblue;
  transition: width 0.3s;
}

.info-content.open {
  width: 40%;
  margin-top: 60px;
}
</style>
