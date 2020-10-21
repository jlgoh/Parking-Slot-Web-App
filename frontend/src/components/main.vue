<template>
  <v-container fill-height align-center justify-center>
    <v-layout row wrap align-center justify-center>
      <v-flex xs10 sm6>
        <v-card>
          <v-layout row align-center justify-center class="py-5">
            <!-- <v-layout row> -->
            <v-flex xs12>
              <div class="text-center mb-5">
                <h1
                  class="text-center mb-5"
                  style="font-weight: 300 !important; font-size:28px;"
                >Welcome!</h1>
                <v-avatar size="128">
                  <img
                    src="https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ0FzOsx49zgl2qnBjOQe2cUSs5dAbvFAiLvjV8M0O0FdKWrOPa6Q"
                    alt="Avatar"
                  />
                </v-avatar>
                <p class="mt-5">To use the features, please login :)</p>
                <v-btn color="primary" outlined type="submit" to="/login">
                  Login
                  <v-icon right>mdi-lock</v-icon>
                </v-btn>
              </div>
              <div class="text-center mb-5">
                <p class="mb-1">Don't have an account?</p>
                <v-btn color="primary" outlined type="submit" to="/register">
                  Register
                  <v-icon right>mdi-account-edit</v-icon>
                </v-btn>
              </div>
              <div class="text-center">
                <p>
                  Lost your password? Reset
                  <v-btn icon to="/forgot">here</v-btn>
                </p>
              </div>
            </v-flex>
          </v-layout>
        </v-card>
      </v-flex>
    </v-layout>
  </v-container>
</template>

