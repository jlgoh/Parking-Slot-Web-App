<template>
  <v-container fill-height>
    <v-layout align-center justify-center>
      <v-flex style="overflow:auto" xs12 sm8 md4>
        <v-card>
          <v-card-text>
            <form @submit.prevent="onLogin">
              <v-container>
                <v-layout row>
                  <v-flex xs12>
                    <v-alert
                      type="error"
                      :value="error"
                    >The username/password is incorrect</v-alert>
                    <h3>Login</h3>
                    <v-text-field
                      name="username"
                      label="Username/Email"
                      id="username"
                      v-model="username"
                      type="text"
                      required
                    ></v-text-field>
                  </v-flex>
                </v-layout>
                <v-layout row>
                  <v-flex xs12>
                    <v-text-field
                      name="password"
                      label="Password"
                      id="password"
                      v-model="password"
                      type="password"
                      required
                    ></v-text-field>
                  </v-flex>
                </v-layout>
              </v-container>
              <v-layout>
                <v-flex xs12>
                  <v-btn color="primary" type="submit">Login</v-btn>
                  <v-btn color="primary" class="ml-5" type="submit" to="/main">Back</v-btn>
                </v-flex>
              </v-layout>
            </form>
          </v-card-text>
        </v-card>
      </v-flex>
    </v-layout>
  </v-container>
</template>

<script>
/* eslint-disable no-unused-vars */
export default {
  data() {
    return {
      username: "",
      password: "",
      confirmPassword: "",
      error: false
    };
  },
  methods: {
    onLogin() {
      /* Send a post req to the backend server */
      /* Set localStorage loginStatus to true */
      /* Redirect the user into the app */
      this.$store
        .dispatch("LOGIN", {
          Username: this.username,
          Password: this.password
        })
        .then(success => {
          this.$router.push("/home");
        })
        .catch(error => {
          this.error = true;
        });
    }
  }
};
</script>
