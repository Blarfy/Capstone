<template>
    <div class="top-bar" :class="{ 'open': isSidebarOpen }">
      <div class="title">{{title}}</div>
      <div class="search-bar">
        <input type="text" placeholder="Search..." v-model="searchQuery" />
      </div>
    </div>
</template>

<script>
export default {
  props: {
    isSidebarOpen: Boolean,
    title: String,
  },
  watch: {
    searchQuery() {
        this.$emit('search-changed', this.searchQuery);
    },
  },
  data() {
    return {
      searchQuery: '',
    };
  },
};
</script>

<style>
.top-bar {
  margin-left: 50px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 15px;
  margin-right: 0;
  background-color: #fff;
  border-bottom: 2px solid black;
  color: black;
  transition: margin-left 0.3s;
}

.top-bar.open {
  margin-left: 200px;
}

.title {
  font-size: 24px;
  font-weight: bold;
}

.search-bar {
  flex: 1;
  text-align: right;
}

.search-bar input {
  padding: 5px;
  width: 23%; 
  border: 1px solid #ccc;
}

</style>