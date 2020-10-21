<template>
  <gmap-map id="google-map" :center="center" ref="mapRef" :zoom="zoom" :options="mapStyle">
    <gmap-marker :position="center" :icon="userMarkerOptions" @click="selectUserLocation()">
      <gmap-info-window :position="center" @closeclick="currentinfo=false" :opened="currentinfo">
        <h2>Current location</h2>
        <v-container class>
          <v-row>
            <span class="body-1">Select a carpark to get more information and directions</span>
          </v-row>
        </v-container>
      </gmap-info-window>
    </gmap-marker>
    <gmap-info-window
      @closeclick="window_open=false"
      :opened="window_open"
      :position="markerPos"
      :options="{
              pixelOffset: {
                width: 0,
                height: -35
              }
            }"
    >
      <h2>
        {{ markerItem.carparkName }}
        <v-chip
          v-if="markerItem.agencyType != '-1'"
          class="ma-2"
          color="primary"
          outlined
        >{{ markerItem.agencyType }}</v-chip>
      </h2>
      <v-container class>
        <v-row v-if="markerItem.address != '-1'">
          <span class="body-1">Address: {{ markerItem.address }}</span>
        </v-row>
        <v-row v-if="markerItem.totalAvailableLots != '-1'">
          <span class="body-1">Total Available Lots: {{ markerItem.totalAvailableLots }}</span>
        </v-row>
        <v-row v-if="markerItem.totalLots != '-1'">
          <span class="body-1">Total Lots: {{ markerItem.totalLots }}</span>
        </v-row>
        <v-row v-if="markerItem.carAvailability != '-1'">
          <span class="body-1">Car Availability: {{ markerItem.carAvailability }}</span>
        </v-row>
        <v-row v-if="markerItem.mAvailability != '-1'">
          <span class="body-1">Motorcycle Availability: {{ markerItem.mAvailability }}</span>
        </v-row>
        <v-row v-if="markerItem.hvAvailability != '-1'">
          <span class="body-1">Heavy Vehicle Availability: {{ markerItem.hvAvailability }}</span>
        </v-row>
        <v-row v-if="markerItem.carCapacity != '-1'">
          <span class="body-1">Car Capacity: {{ markerItem.carCapacity }}</span>
        </v-row>
        <v-row v-if="markerItem.mCapacity != '-1'">
          <span class="body-1">Motorcycle Capacity: {{ markerItem.mCapacity }}</span>
        </v-row>
        <v-row v-if="markerItem.hvCapacity != '-1'">
          <span class="body-1">Heavy Vehicle Capacity: {{ markerItem.hvCapacity }}</span>
        </v-row>
      </v-container>
      <v-btn class="mt-5" color="info" @click="getDirection(markerPos)" type="submit">Directions</v-btn>
    </gmap-info-window>
    <!--<CarparkFilter class="filterBtn" id="filter" @clicked="onFilter"></CarparkFilter>-->
    <v-toolbar id="filterBar" dense floating>
      <v-text-field hide-details prepend-icon="mdi-magnify" single-line></v-text-field>
      <v-btn icon>
        <v-icon color="blue">mdi-crosshairs-gps</v-icon>
      </v-btn>
      <v-btn icon>
        <v-icon>mdi-dots-horizontal</v-icon>
      </v-btn>
    </v-toolbar>
    <template>
      <div class="text-center">
        <v-dialog v-model="sheet" height="90%" width="500px">
          <template v-slot:activator="{ on }">
            <v-btn
              color="purple"
              v-show="route_open"
              id="routeModal"
              style="top:92%;"
              dark
              v-on="on"
            >Show Route</v-btn>
          </template>

          <v-card scrollable>
            <v-card style="border-radius:0;" class="mx-auto mb-5" color="#26c6da" dark>
              <v-card-title>
                <v-icon large left>mdi-routes</v-icon>
                <span class="title font-weight-light">Routing Information</span>
              </v-card-title>
              <v-card-text
                class="headline py-1 font-weight-regular"
              >Start:{{routeInfo.startaddress}}</v-card-text>
              <v-card-text class="headline py-1 font-weight-regular">End: {{routeInfo.endaddress}}</v-card-text>
              <v-card-text
                class="headline py-1 font-weight-regular"
              >Total Distance:{{routeInfo.distance}}</v-card-text>
              <v-card-text
                class="headline py-1 mb-5 font-weight-regular"
              >Total Duration:{{routeInfo.duration}}</v-card-text>
            </v-card>
            <template v-for="(item, index) in steps">
              <v-card-text :key="index" class="ml-4">
                <v-col py-1>
                  <v-badge color="orange" left>
                    <template v-slot:badge>
                      <span>{{index+1}}</span>
                    </template>
                  </v-badge>
                </v-col>
                <v-row>
                  <span class="body-1">Distance: {{item.distance.text}}</span>
                </v-row>
                <v-row>
                  <span class="body-1">Duration: {{item.duration.text}}</span>
                </v-row>
                <v-row>
                  <span class="body-1">Instructions:</span>
                </v-row>
                <v-row>
                  <span class="body-1" v-html="item.instructions"></span>
                </v-row>
                <v-divider></v-divider>
              </v-card-text>
            </template>
          </v-card>
        </v-dialog>
      </div>
    </template>
    <v-btn id="gpsBtn" @click="geolocation" class="ma-2 gmcontrol1">
      <v-icon size="28" color="blue darken-1">mdi-crosshairs-gps</v-icon>
    </v-btn>
  </gmap-map>
</template>

<script>
/*global google*/
import mapStyles from "@/assets/mapStyle";
import * as MarkerClusterer from "marker-clusterer-plus";
import CarparkFilter from "./utils/filter";
const mapMarker = require("../assets/mapmarker.png");
const carMarker = require("../assets/usermarker.png");
var gmap;
var directionsService;
var directionsDisplay;
export default {
  components: {
    CarparkFilter
  },
  data() {
    return {
      center: {
        lat: 1.3521,
        lng: 103.8198
      },
      zoom: 14,
      markers: [],
      markerItem: {},
      carparkItem: {},
      filterConfig: this.$store.getters.FILTER,
      mapStyle: {
        styles: mapStyles,
        disableDefaultUi: false,
        zoomControl: false,
        mapTypeControl: false,
        scaleControl: true,
        streetViewControl: false,
        rotateControl: true,
        fullscreenControl: false
      },
      markerOptions: {
        url: mapMarker,
        size: { width: 30, height: 30 },
        scaledSize: { width: 30, height: 30 }
      },
      userMarkerOptions: {
        url: carMarker,
        size: { width: 40, height: 40 },
        scaledSize: { width: 40, height: 40 }
      },
      currentinfo: true,
      window_open: false,
      markerPos: {
        lat: 1.3521,
        lng: 103.8198
      },
      sheet: false,
      route_open: false,
      steps: [],
      routeInfo: {}
    };
  },
  mounted: function() {
    /* Do not remove */
    /* Need to add mapref on gmaps */
    /* https://github.com/xkjyeah/vue-google-maps/issues/403 */
    this.$refs.mapRef.$mapPromise.then(map => {
      //google api global object - important
      gmap = map;
      this.initGmaps();
      this.geolocation();
      this.fetchCaparks();
    });
  },
  methods: {
    onFilter(filterConfig) {
      //Pass the updated filter config
      //Filter by the markers
      this.filterConfig = filterConfig;
    },
    initGmaps: function() {
      /*   var filterBar = document.getElementById("filter"); */
      var gpsBtn = document.getElementById("gpsBtn");
      var routeModal = document.getElementById("routeModal");
      this.mapObject = this.$refs.mapRef.$mapObject;
      /*this.mapObject.controls[google.maps.ControlPosition.TOP_RIGHT].push(
        filterBar
      );*/
      this.mapObject.controls[google.maps.ControlPosition.RIGHT_BOTTOM].push(
        gpsBtn
      );
      this.mapObject.controls[google.maps.ControlPosition.BOTTOM_CENTER].push(
        routeModal
      );
      //To change the polyline color
      //polylineOptions:{strokeColor:"#4a4a4a",strokeWeight:5},
      directionsDisplay = new google.maps.DirectionsRenderer({
        suppressMarkers: true,
        polylineOptions: { strokeColor: "green", strokeWeight: 5 }
      });
    },
    geolocation: function() {
      navigator.geolocation.getCurrentPosition(position => {
        this.center = {
          lat: parseFloat(position.coords.latitude),
          lng: parseFloat(position.coords.longitude)
        };
        this.$refs.mapRef.panTo(this.center);
      });
    },
    selectUserLocation: function() {
      this.currentinfo = !this.currentinfo;
      this.$refs.mapRef.panTo(this.center);
    },
    fetchCaparks: function() {
      let cur = this;
      this.axios
        .get("https://parkingslotapi.azurewebsites.net/api/carpark/all")
        .then(function(response) {
          var markers = [];
          for (var i = 0; i < response.data.length; i++) {
            //Remove zero coordinates marker
            if (
              response.data[i].xCoord != "0" &&
              response.data[i].yCoord != "0"
            ) {
              var userloc = {
                position: {
                  id: response.data[i].id,
                  carparkId: response.data[i].carparkId,
                  lat: parseFloat(response.data[i].xCoord),
                  lng: parseFloat(response.data[i].yCoord)
                }
              };
              var marker = new google.maps.Marker({
                position: userloc.position,
                map: gmap,
                icon: cur.markerOptions
              });
              cur.addClickEvent(marker, userloc.position);
              markers.push(marker);
            }
          }
          var markerCluster = new MarkerClusterer(gmap, markers);
        });
    },
    fetchCarparkItem: function(id) {
      let cur = this;
      this.axios
        .get("https://parkingslotapi.azurewebsites.net/api/carpark/" + id)
        .then(function(response) {
          cur.markerItem = response.data;
        });
    },
    addClickEvent: function(marker, markerInfo) {
      //Add a listener for each marker
      //Render information on one infowindow
      //Update the position
      let cur = this;
      marker.addListener("click", function() {
        cur.window_open = true;
        cur.currentinfo = false;
        cur.route_open = false;
        var position = {
          lat: parseFloat(markerInfo.lat),
          lng: parseFloat(markerInfo.lng)
        };
        cur.markerPos = position;
        cur.$refs.mapRef.panTo(cur.markerPos);
        cur.fetchCarparkItem(markerInfo.id);
      });
    },
    getDirection: function(markerInfo) {
      //Close the infowindow
      let cur = this;
      this.window_open = false;
      var destination = new Object();
      destination.lat = parseFloat(markerInfo.lat);
      destination.lng = parseFloat(markerInfo.lng);
      directionsService = new google.maps.DirectionsService();
      //Remove previous routing
      directionsDisplay.setMap(this.$refs.mapRef.$mapObject);
      directionsService.route(
        {
          origin: this.center,
          destination: destination,
          travelMode: "DRIVING"
          /* More configurations
          transitOptions: TransitOptions,
          drivingOptions: DrivingOptions,
          unitSystem: UnitSystem,
          waypoints[]: DirectionsWaypoint,
          optimizeWaypoints: Boolean,
          provideRouteAlternatives: Boolean,
          avoidFerries: Boolean,
          avoidHighways: Boolean,
          avoidTolls: Boolean,
          */
        },
        function(response, status) {
          if (status === "OK") {
            cur.route_open = true;
            directionsDisplay.setDirections(response);
            cur.steps = response.routes[0].legs[0].steps;
            cur.routeInfo.startaddress =
              response.routes[0].legs[0].start_address;
            cur.routeInfo.endaddress = response.routes[0].legs[0].end_address;
            cur.routeInfo.distance = response.routes[0].legs[0].distance.text;
            cur.routeInfo.duration = response.routes[0].legs[0].duration.text;
          } else {
            console.error("Directions request failed due to " + status);
          }
        }
      );
      console.log("Finished getting the route!");
    }
  }
};
</script>
<style>
/* Infowindow styling */
.gm-style-iw {
  width: 350px !important;
  background-color: #fff !important;
  box-shadow: 0 1px 6px rgba(178, 178, 178, 0.6);
}

.gmcontrol1 {
  position: absolute;
  background-color: white;
  top: 90%;
  right: 0;
  bottom: 0;
  z-index: 10;
  width: 40px !important;
  height: 40px !important;
}

#google-map {
  width: 100%;
  height: 100%;
}

#filterBar {
  width: 300px;
  margin-top: 10px !important;
  margin: 0 auto;
}
</style>