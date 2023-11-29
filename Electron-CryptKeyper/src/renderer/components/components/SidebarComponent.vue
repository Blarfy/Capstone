<template>
  <div class="sidebar" :class="{ 'open': isSidebarOpen }">
    <nav :style="{ width: isSidebarOpen ? '200px' : '0' }">
      <ul>
        <li></li>
        <!-- On click, emit a changeView -->
        <li><a @click="$emit('changeView', 'password')">Passwords</a></li>
        <li><a @click="$emit('changeView', 'notes')">Notes</a></li>
        <li><a @click="$emit('changeView', 'payment')">Payment</a></li>
        <li><a @click="$emit('changeView', 'files')">Files</a></li>
        <li><a @click="$emit('changeView', 'shared')">Shared</a></li>
      <li></li>
      <li></li>
      <li></li>
      <li></li>
      <li></li>
      <li><a href="#:>">:></a></li>
      <li><a @click="$emit('reload')">Refresh</a></li>
      <li><a @click="logout">Logout</a></li>
      </ul>
    </nav>
    <button @click="toggleSidebar" class="toggle-button">&#9776;</button>
  </div>
</template>

<script>
export default {
  props: ['isSidebarOpen'],
  methods: {
    toggleSidebar() {
      this.$emit('toggleSidebar'); // Emit an event to toggle the sidebar in the parent
    },
    logout() {
      this.$emit('logout');
      this.toggleSidebar();
    },
  },
};
</script>

<style scoped>
.sidebar {
    width: 50px;
    height: 100%;
    position: fixed;
    top: 0;
    left: 0;
    background-color: #333;
    color: #fff;
    transition: width 0.3s, left 0.3s, background-color 0.3s;
}

nav {
    width: 0;
    height: 100%;
    overflow: hidden;
    position: absolute;
    background-color: #333;
    transition: width 0.3s;
}

nav.open {
    width: 200px;
}

.toggle-button {
    background-color: #333;
    border: none;
    color: #fff;
    padding: 10px 5px;
    font-size: 20px;
    cursor: pointer;
    position: absolute;
    top: 0;
}

.toggle-button:focus {
    outline: none;
}

nav ul {
    list-style: none;
    padding: 0;
}

nav ul li {
    padding: 10px;
}

nav ul li a {
    color: gainsboro;
    text-decoration: none;
    cursor: pointer;
}
</style>
