  
<template>
  <div class="navbar">
    <!-- Navbar -->
    <v-app-bar app dense fixed dark color="primary">
      <v-app-bar-nav-icon @click="navdraw = !navdraw"></v-app-bar-nav-icon>
      <v-spacer></v-spacer>
      <v-toolbar-title>
        <router-link to="/" tag="span" class="tb-title">ParkingSlot</router-link>
      </v-toolbar-title>
      <div class="flex-grow-1"></div>
      <v-menu v-model="feedbackMenu" :close-on-content-click="false" :nudge-width="200" offset-x>
        <template v-slot:activator="{ on }">
          <v-btn v-on="on" icon>
            <v-icon>mdi-comment-question-outline</v-icon>
          </v-btn>
        </template>
        <v-card>
          <v-list>
            <v-list-item>
              <v-list-item-avatar>
                <v-icon class="mdi-48px">mdi-account-box-outline</v-icon>
              </v-list-item-avatar>
              <v-list-item-content>
                <v-list-item-title>Facing any difficulties?</v-list-item-title>
                <v-list-item-subtitle>Feel free to leave a feedback! :)</v-list-item-subtitle>
              </v-list-item-content>
            </v-list-item>
          </v-list>
          <v-divider></v-divider>
          <v-container>
            <v-container>
              <v-row>
                <v-col cols="12">
                  <v-text-field v-model="newFeedback.topic" label="Topic" outlined></v-text-field>
                </v-col>
              </v-row>
              <v-row>
                <v-col cols="12">
                  <v-textarea
                    v-model="newFeedback.description"
                    auto-grow
                    label="Description"
                    outlined
                  ></v-textarea>
                </v-col>
              </v-row>
            </v-container>
          </v-container>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="primary" text @click="submitUserFeedback">Save</v-btn>
            <v-btn text @click="cancelFeedback">Cancel</v-btn>
          </v-card-actions>
        </v-card>
      </v-menu>
    </v-app-bar>
    <v-navigation-drawer temporary v-model="navdraw" app>
      <template v-slot:prepend v-if="login">
        <v-list-item two-line>
          <v-list-item-avatar>
            <img src="https://randomuser.me/api/portraits/men/81.jpg" />
          </v-list-item-avatar>
          <v-list-item-content>
            <v-list-item-title>{{ username }}</v-list-item-title>
            <v-list-item-subtitle>Logged In</v-list-item-subtitle>
          </v-list-item-content>
        </v-list-item>
        <v-divider></v-divider>
      </template>
      <!-- Login view sidebar -->
      <v-list dense v-if="login">
        <v-list-item @click="redirect('home')">
          <v-list-item-icon>
            <v-icon>mdi-home</v-icon>
          </v-list-item-icon>
          <v-list-item-content>
            <v-list-item-title>Home</v-list-item-title>
          </v-list-item-content>
        </v-list-item>
        <v-list-item @click="redirect('profile')">
          <v-list-item-icon>
            <v-icon>mdi-face</v-icon>
          </v-list-item-icon>
          <v-list-item-content>
            <v-list-item-title>Profile</v-list-item-title>
          </v-list-item-content>
        </v-list-item>
        <v-list-item @click="redirect('favourite')">
          <v-list-item-icon>
            <v-icon>mdi-heart</v-icon>
          </v-list-item-icon>
          <v-list-item-content>
            <v-list-item-title>Favourite</v-list-item-title>
          </v-list-item-content>
        </v-list-item>
        <v-list-item @click="redirect('map')">
          <v-list-item-icon>
            <v-icon>mdi-map-marker</v-icon>
          </v-list-item-icon>
          <v-list-item-content>
            <v-list-item-title>Map</v-list-item-title>
          </v-list-item-content>
        </v-list-item>
        <v-divider></v-divider>
        <v-list-item @click="logout()">
          <v-list-item-icon>
            <v-icon>mdi-lock-open</v-icon>
          </v-list-item-icon>
          <v-list-item-content>
            <v-list-item-title>Logout</v-list-item-title>
          </v-list-item-content>
        </v-list-item>
        <br />
      </v-list>

      <!-- Not logged in view sidebar -->
      <v-list dense v-if="!login">
        <v-list-item @click="redirect('login')">
          <v-list-item-icon>
            <v-icon>mdi-account</v-icon>
          </v-list-item-icon>
          <v-list-item-content>
            <v-list-item-title>Login</v-list-item-title>
          </v-list-item-content>
        </v-list-item>
        <v-list-item @click="redirect('register')">
          <v-list-item-icon>
            <v-icon>mdi-playlist-plus</v-icon>
          </v-list-item-icon>
          <v-list-item-content>
            <v-list-item-title>Register</v-list-item-title>
          </v-list-item-content>
        </v-list-item>
        <v-divider></v-divider>
        <v-list-item @click="redirect('information')">
          <v-list-item-icon>
            <v-icon>mdi-information</v-icon>
          </v-list-item-icon>
          <v-list-item-content>
            <v-list-item-title>More information</v-list-item-title>
          </v-list-item-content>
        </v-list-item>
      </v-list>
    </v-navigation-drawer>
  </div>
</template>

<script>
export default {
  data() {
    return {
      navdraw: false,
      feedbackMenu: false,
      newFeedback: {}
    };
  },
  computed: {
    login() {
      let status = this.$store.getters.ISLOGGEDIN;
      return status;
    },
    username() {
      let username = this.$store.getters.USERNAME;
      return username;
    }
  },
  methods: {
    redirect(url) {
      this.$router.push("/" + url);
    },
    logout() {
      this.$store.commit("SETAUTHSTATUS", false);
      localStorage.clear();
      this.$router.push({ path: "/login" });
    },
    submitUserFeedback() {
      this.$store
        .dispatch("CREATEFEEDBACK", {
          topic: this.newFeedback.topic,
          description: this.newFeedback.description
        })
        .then(success => {
          this.feedbackMenu = false;
          alert("Feedback created");
        })
        .catch(error => {
          this.error = true;
        });
    },
    cancelFeedback(){
      this.feedbackMenu = false;
      this.newFeedback = {};
    }
  }
};
</script>