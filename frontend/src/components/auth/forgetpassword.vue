<template>
  <v-container fill-height>
    <v-layout align-center justify-center>
      <v-flex style="overflow:auto" xs12 sm8 md4>
        <v-card>
          <v-card-text>
            <form @submit.prevent="getResetEmail">
              <v-container>
                <v-layout row>
                  <v-flex xs12>
                    <v-alert
                      dismissible
                      type="success"
                      :value="success"
                    >Check your email ({{email}}) to reset your password!</v-alert>
                    <v-alert dismissible type="error" :value="error">The email is invalid</v-alert>
                    <h3>Forgot your password?</h3>
                    <v-text-field
                      name="username"
                      label="Enter Email"
                      id="username"
                      v-model="email"
                      type="text"
                      required
                    ></v-text-field>
                  </v-flex>
                </v-layout>
              </v-container>
              <v-layout>
                <v-flex xs12>
                  <v-btn color="primary" type="submit">Reset</v-btn>
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
      email: "",
      error: false,
      success: false
    };
  },
  methods: {
    getResetEmail() {
      this.axios
        .post(
          "https://parkingslotapi.azurewebsites.net/api/users/forgetpassword",
          {
            email: this.email
          }
        )
        .then(response => {
          if (response.status == "200") {
            this.success = true;
            this.error = false;
          } else {
            this.success = false;
            this.error = true;
          }
        });
    }
  }
};
</script>
