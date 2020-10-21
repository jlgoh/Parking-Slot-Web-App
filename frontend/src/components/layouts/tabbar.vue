<template>
  <div class="tabbar">
    <v-bottom-navigation dark fixed grow background-color="primary" color="white">
      <v-btn to="/home">
        <span>Home</span>
        <v-icon>mdi-home</v-icon>
      </v-btn>
      <v-btn to="/map">
        <span>Map</span>
        <v-icon>mdi-map-marker</v-icon>
      </v-btn>
      <v-btn to="/favourite">
        <span>Favorites</span>
        <v-icon>mdi-heart</v-icon>
      </v-btn>
      <v-btn to="/profile">
        <span>Profile</span>
        <v-icon>mdi-face</v-icon>
      </v-btn>
      <v-btn v-if="this.$store.getters.ROLE == 'Admin'" to="/admin">
        <span>Admin</span>
        <v-icon>mdi-auto-fix</v-icon>
      </v-btn>
    </v-bottom-navigation>
  </div>
</template>