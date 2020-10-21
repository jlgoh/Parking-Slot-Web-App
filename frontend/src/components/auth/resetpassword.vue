<template>
  <v-content>
    <v-container fluid>
      <v-layout column>
        <v-card>
          <v-alert
            type="error"
            :value="passwordError"
          >{{passwordErrorText}}</v-alert>
          <v-alert
            :value="notifyStatus"
            dismissible
            type="success"
          >Your password have been changed successfully!</v-alert>
          <v-card-text>
            <v-text-field
              name="password"
              v-model="userPassword.newpassword"
              label="New password"
              type="password"
            ></v-text-field>
            <v-text-field
              name="cfmpassword"
              v-model="userPassword.cfmnewpassword"
              label="Confirm new password"
              type="password"
            ></v-text-field>
          </v-card-text>
          <v-card-actions>
            <v-btn color="primary" v-if="!updated" @click="changePassword">
              <v-icon left dark>mdi-check</v-icon>Change Password
            </v-btn>
            <v-btn color="primary" v-if="updated" to="/login">
              <v-icon left dark>mdi-check</v-icon>Login
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-layout>
    </v-container>
  </v-content>
</template>

<script>
export default {
  data() {
    return {
      updated: false,
      userPassword: {
        newpassword: "",
        cfmnewpassword: ""
      },
      notifyStatus: false,
      passwordError: false,
      passwordErrorText: ""
    };
  },
  created: function() {
    var token = this.$route.params.token;
    this.$store.dispatch("CHECKTOKEN", token).catch(error => {
      this.$router.push("/main");
      this.error = true;
    });
  },
  methods: {
    changePassword() {
      if (this.userPassword.newpassword != this.userPassword.cfmnewpassword) { //pw do not match
        this.passwordError = true;
        this.passwordErrorText = "New passwords do not match.";
        return;
      }

      //update password
      var userId = this.$route.params.userId;
      var update = {
        Id: userId,
        NewPassword: this.userPassword.newpassword
      };
      this.$store.dispatch("SETNEWPASSWORD", update).then(response => {
        if (response == true) {
          this.passwordError = false;
          this.notifyStatus = true;
          this.updated = true;
        }
      })
      .catch(error => {
          console.log(error.response);
          this.passwordError = true;
          this.passwordErrorText = "Passwords must be at least 8 characters and contain at 3 of 4 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)";
        });
    }
  }
};
</script>
