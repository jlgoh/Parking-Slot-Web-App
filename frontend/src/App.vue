<template>
  <v-layout>
    <v-app class="grey lighten-4 main">
      <navbar></navbar>
      <router-view class="content"></router-view>
      <tabbar v-show="this.$store.getters.ISLOGGEDIN"></tabbar>
    </v-app>
  </v-layout>
</template>
<style>
@import url("./assets/style.css");
</style>
<script>
import navbar from "./components/layouts/navbar";
import tabbar from "./components/layouts/tabbar";
export default {
  components: {
    navbar,
    tabbar
  }
};
</script>
<style>
.main {
  height: 100% !important;
  width: 100% !important;
}
.content{
  margin-top: 48px;
  margin-bottom: 55px;
}
</style>