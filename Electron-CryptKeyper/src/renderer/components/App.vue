<template>
  <div>
    <login-component></login-component>
    <sidebar-component :isSidebarOpen="isSidebarOpen" @toggleSidebar="toggleSidebar" />
    <content-component :isSidebarOpen="isSidebarOpen" />
  </div> 
</template>

<script>
import LoginComponent from './LoginPage.vue';
import SidebarComponent from './components/SidebarComponent.vue';
import ContentComponent from './components/ContentComponent.vue';
import { ref } from 'vue';

export default {
  name: 'App',
  components: {
    LoginComponent,
    SidebarComponent,
    ContentComponent,
  },
  setup() {
    const isSidebarOpen = ref(false);

    const toggleSidebar = () => {
      isSidebarOpen.value = !isSidebarOpen.value;
    };

    return {
      isSidebarOpen,
      toggleSidebar,
    };
  },
};
</script>

<style>
body {
    margin: 0;
    padding: 0;
    font-family: Arial, sans-serif;
    display: inline;
    background-color: gainsboro;
}
</style>
