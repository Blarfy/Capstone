<template>
  <div>
    <login-component @userLogin="userLogin"></login-component>
    <sidebar-component :isSidebarOpen="isSidebarOpen" @toggleSidebar="toggleSidebar" />
    <content-component :userLoginfo="userLoginfo" :isSidebarOpen="isSidebarOpen" />
  </div> 
</template>

<script>
import LoginComponent from './components/Login.vue';
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
    const userLoginfo = ref(null);

    const userLogin = (email, password, key) => {
      console.log('User logged in');
      userLoginfo.value = {email, password, key};
    };

    const toggleSidebar = () => {
      isSidebarOpen.value = !isSidebarOpen.value;
    };

    return {
      isSidebarOpen,
      toggleSidebar,
      userLoginfo,
      userLogin
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
