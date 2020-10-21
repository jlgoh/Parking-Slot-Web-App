<template>
  <v-card>
    <v-list two-line>
      <v-layout justify-center v-if="displayMsg">
        <h3 style="font-size:22px;font-weight:300;">No carpark favorites... 😱</h3>
      </v-layout>
      <v-list-item-group>
        <template v-for="(item, index) in items">
          <v-list-item :key="item.id" @click.stop="displayCarparkInfo(item)">
            <template>
              <v-list-item-content>
                <v-list-item-title v-text="item.carparkName"></v-list-item-title>
                <v-list-item-subtitle v-text="item.carparkLocation"></v-list-item-subtitle>
              </v-list-item-content>
              <v-list-item-action>
                <v-icon @click.stop="unfavourite(item.id)">mdi-delete</v-icon>
              </v-list-item-action>
            </template>
          </v-list-item>
          <v-divider v-if="index + 1 < items.length" :key="index"></v-divider>
        </template>
      </v-list-item-group>
    </v-list>
    <v-dialog v-model="dialog" width="500">
      <v-card>
        <v-card-title class="headline blue font-weight-light darken-2" style="color:white" primary-title>Carpark Information</v-card-title>
        <v-card-text>
          <v-container>
            <v-row>
              <span class="body-1">Carpark ID: {{ viewingItem.carparkId }}</span>
            </v-row>
            <v-row>
              <span class="body-1">Carpark Name: {{ viewingItem.carparkName }}</span>
            </v-row>
            <v-row>
              <span class="body-1">Agency Type: {{ viewingItem.agencyType }}</span>
            </v-row>
            <v-row>
              <span class="body-1">Address: {{ viewingItem.carparkLocation }}</span>
            </v-row>
            <v-row v-if="viewingItem.totalAvailableLots != '-1'">
              <span class="body-1">Total Available Lots: {{ viewingItem.totalAvailableLots }}</span>
            </v-row>
            <v-row v-if="viewingItem.totalLots != '-1'">
              <span class="body-1">Total Lots: {{ viewingItem.totalLots }}</span>
            </v-row>
            <v-row v-if="viewingItem.carAvailability != '-1'">
              <span class="body-1">Car Availability: {{ viewingItem.carAvailability }}</span>
            </v-row>
            <v-row v-if="viewingItem.mAvailability != '-1'">
              <span class="body-1">Motorcycle Availability: {{ viewingItem.mAvailability }}</span>
            </v-row>
            <v-row v-if="viewingItem.hvAvailability != '-1'">
              <span class="body-1">Heavy Vehicle Availability: {{ viewingItem.hvAvailability }}</span>
            </v-row>
            <v-row v-if="viewingItem.carCapacity != '-1'">
              <span class="body-1">Car Capacity: {{ viewingItem.carCapacity }}</span>
            </v-row>
            <v-row v-if="viewingItem.mCapacity != '-1'">
              <span class="body-1">Motorcycle Capacity: {{ viewingItem.mCapacity }}</span>
            </v-row>
            <v-row v-if="viewingItem.hvCapacity != '-1'">
              <span class="body-1">Heavy Vehicle Capacity: {{ viewingItem.hvCapacity }}</span>
            </v-row>
            <br />
            <v-row v-if="viewingItem.carRates != 0">
              <span style="text-decoration: underline !important;" class="body-1">Car Rates:</span>
            </v-row>
            <v-row v-for="rate in viewingItem.carRates" :key="rate.id">
              <span class="body-1">Time Range: {{rate.startTime}} - {{rate.endTime}}</span>
              <span class="body-1">Weekday Rate: {{rate.weekdayRate}} per 30 min</span>
              <span class="body-1">Saturday Rate: {{rate.satdayRate}} per 30 min</span>
              <span class="body-1 mb-5">Sunday/Public Holiday Rate: {{rate.sunPHRate}} per 30 min</span>
            </v-row>
            <v-row v-if="viewingItem.motorRates != 0">
              <span style="text-decoration: underline !important;" class="body-1">Motorcycle Rates:</span>
            </v-row>
            <v-row class="row" v-for="rate in viewingItem.motorRates" :key="rate.id">
              <span class="body-1">Time Range: {{rate.startTime}} - {{rate.endTime}}</span>
              <span class="body-1">Weekday Rate: {{rate.weekdayRate}} per 30 min</span>
              <span class="body-1">Saturday Rate: {{rate.satdayRate}} per 30 min</span>
              <span class="body-1 mb-5">Sunday/Public Holiday Rate: {{rate.sunPHRate}} per 30 min</span>
            </v-row>
            <v-row v-if="viewingItem.hvRates != 0">
              <span style="text-decoration: underline !important;" class="body-1">Heavy Vehicle Rates:</span>
            </v-row>
            <v-row class="row" v-for="rate in viewingItem.hvRates" :key="rate.id">
              <span class="body-1">Time Range: {{rate.startTime}} - {{rate.endTime}}</span>
              <span class="body-1">Weekday Rate: {{rate.weekdayRate}} per 30 min</span>
              <span class="body-1">Saturday Rate: {{rate.satdayRate}} per 30 min</span>
              <span class="body-1 mb-5">Sunday/Public Holiday Rate: {{rate.sunPHRate}} per 30 min</span>
            </v-row>
          </v-container>
        </v-card-text>
        <v-card-actions>
          <div class="flex-grow-1"></div>
          <v-btn color="primary" text @click="dialog = false">Go Back</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-card>
</template>
<script>
export default {
  data() {
    return {
      dialog: false,
      displayMsg: true,
      selected: [],
      items: [],
      viewingItem: {}
    };
  },
  created: function() {
    this.fetchFavoritesData();
  },
  methods: {
    fetchFavoritesData: function() {
      //Retrieve favorites data (CarparkID)
      //Get request from backend first
      this.$store
        .dispatch("GETFAVORITES")
        .then(success => {
          var idArr = this.$store.getters.FAVORITES.slice();
          if (idArr.length > 0) {
            this.displayMsg = false;
            idArr.forEach(carparkID => {
              this.fetchCarparkInfo(carparkID);
            });
          } else {
            this.displayMsg = true;
          }
        })
        .catch(error => {
          console.log(error);
        });
    },
    unfavourite: function(carparkID) {
      var cur = this;
      //Delete Request
      if (confirm("Are you sure?")) {
        //Send Delete Request to Backend: If succesful, then delete from frontend
        this.$store
          .dispatch("DELETEFAVORITE", carparkID)
          .then(success => {
            cur.items.forEach((item, index) => {
              if (item.id == carparkID) {
                cur.items.splice(index, 1);
              }
            });
            if (cur.items.length == 0) {
              this.displayMsg = true;
            }
          })
          .catch(error => {
            console.log(error);
          });
      }
    },
    fetchCarparkInfo: function(carparkID) {
      let cur = this;
      this.axios
        .all([
          this.axios.get(
            `https://parkingslotapi.azurewebsites.net/api/carpark/${carparkID}` //Retrieve Carpark information from GetCarpark API call using CarparkID
          ),
          this.axios.get(
            `https://parkingslotapi.azurewebsites.net/api/CarparkRates/${carparkID}`
          )
        ]) //Retrieve Carpark Rates from GetCarpark API call using CarparkID
        .then(
          this.axios.spread((response, response2) => {
            var MotorcycleRates = [];
            var HVRates = [];
            var CarRates = [];
            if (response.status == "200" && response2.status == "200") {
              response2.data.forEach(rate => {
                if (rate.vehicleType == "Motorcycle") {
                  MotorcycleRates.push({
                    startTime: rate.startTime,
                    endTime: rate.endTime,
                    weekdayRate: rate.weekdayRate,
                    satdayRate: rate.satdayRate,
                    sunPHRate: rate.sunPHRate
                  });
                }
                if (rate.vehicleType == "Heavy Vehicle") {
                  HVRates.push({
                    startTime: rate.startTime,
                    endTime: rate.endTime,
                    weekdayRate: rate.weekdayRate,
                    satdayRate: rate.satdayRate,
                    sunPHRate: rate.sunPHRate
                  });
                }
                if (rate.vehicleType == "Car") {
                  CarRates.push({
                    startTime: rate.startTime,
                    endTime: rate.endTime,
                    weekdayRate: rate.weekdayRate,
                    satdayRate: rate.satdayRate,
                    sunPHRate: rate.sunPHRate
                  });
                }
              });

              if (CarRates == []) CarRates = 0; //So that can clear in v-if
              if (MotorcycleRates == []) MotorcycleRates = 0; //So can clear in v-if
              if (HVRates == []) HVRates = 0;

              cur.items.push({
                id: response.data.id,
                carparkId: response.data.carparkId,
                carparkName: response.data.carparkName,
                lotType: response.data.lotType,
                agencyType: response.data.agencyType,
                carparkLocation: response.data.address,
                totalAvailableLots: response.data.totalAvailableLots,
                totalLots: response.data.totalLots,
                carAvailability: response.data.carAvailability,
                mAvailability: response.data.mAvailability,
                hvAvailability: response.data.hvAvailability,
                carCapacity: response.data.carCapacity,
                mCapacity: response.data.mCapacity,
                hvCapacity: response.data.hvCapacity,
                motorRates: MotorcycleRates,
                carRates: CarRates,
                hvRates: HVRates
              });
            }
          })
        );
    },
    displayCarparkInfo: function(item) {
      this.viewingItem = item;
      this.dialog = true;
    }
  }
};
</script>