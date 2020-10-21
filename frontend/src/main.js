import Vue from 'vue'
import App from './App.vue'
import router from './router'

//Initialize all the Javascript Libraries here
/* 1. Promise based HHTP client for the browser */
import axios from 'axios'
import VueAxios from 'vue-axios'
/* 2. Vue Material Design Component Framework*/
import vuetify from './plugins/vuetify';
/* 3. Vuex state management */
import store from './stores/store'
/* 4. Vue2-google-maps */
import * as VueGoogleMaps from 'vue2-google-maps'
/* 5. Gmap clusters */
import GmapCluster from 'vue2-google-maps/dist/components/cluster'
/* 6. Infinite loading */
import InfiniteLoading from 'vue-infinite-loading';
/* 7. NProgress */
import NProgress from 'nprogress';
import 'nprogress/nprogress.css'
/* 8. Datetimepicker */
import DatetimePicker from 'vuetify-datetime-picker'
/* 9. Time Parser */
import moment from "moment";

// Install Javascript Libraries
// Must be called before new Vue()
Vue.use(VueAxios, axios);
Vue.use(VueGoogleMaps, {
  installComponents: true,
  load: {
    key: 'AIzaSyA78_ixDHcOmN-gmzu5nYVkBcFD0A8vkw8',
    libraries: 'places'
  }
})
Vue.component('GmapCluster', GmapCluster)
Vue.use(InfiniteLoading, {
  props: {
    spinner: 'default',
    /* other props need to configure */
  },
  system: {
    throttleLimit: 500,
    /* other settings need to configure */
  },
});
Vue.use(DatetimePicker);
Vue.prototype.moment = moment;
Vue.config.productionTip = false


axios.interceptors.request.use(
  config => {
    NProgress.start();
    const token = localStorage.getItem("access_token");
    if (token) {
      config.headers["Authorization"] = "Bearer " + token;
    }
    return config;
  },
  error => {
    Promise.reject(error);
  }
);

axios.interceptors.response.use(function (response) {
  NProgress.done();
  return response;
}, function (error) {
  NProgress.done();
  return Promise.reject(error);
})

new Vue({
  router,
  vuetify,
  store,
  render: h => h(App)
}).$mount('#app')
