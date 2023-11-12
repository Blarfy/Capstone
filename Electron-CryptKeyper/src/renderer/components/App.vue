<template>
  <div>
    <login-component @userLogin="userLogin" @changeView="changeView"></login-component>
    <sidebar-component :isSidebarOpen="isSidebarOpen" @toggleSidebar="toggleSidebar" @changeView="changeView"/>
    <password-view v-show="viewPassword" :userLoginfo="userLoginfo" :isSidebarOpen="isSidebarOpen" />
    <notes-view v-show="viewNotes" :userLoginfo="userLoginfo" :isSidebarOpen="isSidebarOpen" />
    <payment-view v-show="viewPayment" :userLoginfo="userLoginfo" :isSidebarOpen="isSidebarOpen" />
  </div> 
</template>

<script>
import LoginComponent from './components/Login.vue';
import SidebarComponent from './components/SidebarComponent.vue';
import PasswordView from './components/PasswordView.vue';
import NotesView from './components/NotesView.vue';
import PaymentView from './components/PaymentView.vue';
import { ref } from 'vue';

export default {
  name: 'App',
  components: {
    LoginComponent,
    SidebarComponent,
    PasswordView,
    NotesView,
    PaymentView
  },
  data() {
    return {
      viewPassword: false,
      viewNotes: false,
      viewPayment: false,
      viewFiles: false,
    }
  },
  methods: {
    changeView(chosen) {
      switch (chosen) {
        case 'password':
          this.viewPassword = true;
          this.viewNotes = false;
          this.viewPayment = false;
          this.viewFiles = false;
          break;
        case 'notes':
          this.viewPassword = false;
          this.viewNotes = true;
          this.viewPayment = false;
          this.viewFiles = false;
          break;
        case 'payment':
          this.viewPassword = false;
          this.viewNotes = false;
          this.viewPayment = true;
          this.viewFiles = false;
          break;
        case 'files':
          this.viewPassword = false;
          this.viewNotes = false;
          this.viewPayment = false;
          this.viewFiles = true;
          break;
      }
    }
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


.content {
  margin-left: 50px;
  transition: margin-left 0.3s;
  display: flex;
}

.content.open {
  margin-left: 200px;
}

.column {
  margin-top: 20px;
  padding: 15px;
  width: 25%;
  /* max-width: 30%; */
  /* Add flexbox stuff here, use space-evenly to have columns placed properly */
  border: 1px solid #ccc;
  flex: 1;
}

.column h2 {
  font-weight: bold;
  font-size: 28px;
  border-bottom: 2px solid darkgray;
  text-align: center;
}
.plus-button { 
	border: 2px solid lightgrey;
	background-color: darkslategrey;
  color: white;
	font-size: 48px;
  font-weight: bolder;
	height: 1.8em;
	width: 1.8em;
	border-radius: 999px;
	position: absolute;
  align-self: right;
  right: 50px;
  bottom: 50px;
	
	&:after,
	&:before {
		content: "";
		display: block;
		background-color: grey;
		position: absolute;
		top: 50%;
		left: 50%;
		transform: translate(-50%, -50%);
	}
	
	&:before {
		height: 1em;
		width: 0.2em;
	}

	&:after {
		height: 0.2em;
		width: 1em;
	}
}

@media (max-width: 768px) {
  .column {
    flex: 0 0 20%; /* On smaller screens, make each column take up 20% of the width */
  }
}
</style>
