Vue.component("sidebar-component", {
  props: {
    isSidebarOpen: Boolean,
  },
  template: `
    <div class="sidebar" :class="{ 'open': isSidebarOpen }">
      <nav :style="{ width: navWidth }">
        <ul>
          <li/>
          <li/>
          <li><a href="#">Home</a></li>
          <li><a href="#">About</a></li>
          <li><a href="#">Services</a></li>
          <li><a href="#">Contact</a></li>
        </ul>
      </nav>
      <button @click="toggleSidebar" class="toggle-button">
        &#9776;
      </button>
    </div>
  `,
  computed: {
    navWidth() {
      return this.isSidebarOpen ? "200px" : "0";
    },
  },
  methods: {
    toggleSidebar() {
      this.$root.toggleSidebar(); // Call the parent's toggleSidebar method
    },
  },
});

Vue.component("content-component", {
  props: {
    isSidebarOpen: Boolean,
  },
  template: `
    <div class="content" :class="{ 'open': isSidebarOpen }">
      <h1>Welcome to My Website</h1>
      <p>This is the main content of your website.</p>
    </div>
  `,
  computed: {
    conWidth() {
      return this.isSidebarOpen ? "calc(100% - 200px)" : "100%";
    },
  },
});

// Create a new Vue instance
new Vue({
  el: "#app",
  data: {
    isSidebarOpen: false,
  },
  methods: {
    toggleSidebar() {
      this.isSidebarOpen = !this.isSidebarOpen;
    },
  },
});
