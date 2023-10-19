import { createApp } from "vue"; // Import createApp for Vue 3
import App from "./App.vue";
import SidebarComponent from "./components/SidebarComponent.vue";
import ContentComponent from "./components/ContentComponent.vue";
import Login from "./components/Login.vue";

// Create a Vue app instance
const app = createApp(App);

// Register components
app.component("sidebar-component", SidebarComponent);
app.component("content-component", ContentComponent);
app.component("login-component", Login)

// Mount the app
app.mount("#app");
