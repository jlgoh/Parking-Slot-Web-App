<template>
  <v-flex xs12>
    <v-card-text class="filter">
      <v-menu offset-y>
        <template v-slot:activator="{ on }">
          <v-layout>
            <v-text-field
              style="margin-top:10px; margin-right: 15px;"
              placeholder="Enter carpark name"
              v-model.lazy="filterText"
              outlined
              background-color="white"
            ></v-text-field>
            <v-btn
              align-end
              justify-end
              style="margin-right: -5px !important; background-color: white;"
              class="ma-2"
              v-on="on"
              @click="openFilter"
              outlined
              fab
              color="teal"
            >
              <v-icon v-if="isFiltered">mdi-filter</v-icon>
              <v-icon v-if="!isFiltered">mdi-filter-outline</v-icon>
            </v-btn>
          </v-layout>
        </template>
        <v-card max-width="500" outlined>
          <v-list-item>
            <v-list-item-content @click.stop>
              <div class="filterheader mb-4">Filter carparks by:</div>
              <v-col cols="12" sm="6" md="6">
                <v-switch v-model="filterConfig.IsAscending" label="Ascending Order"></v-switch>
              </v-col>
              <v-col cols="12" sm="6" md="6">
                <v-switch v-model="filterConfig.IsElectronic" label="Electronic Parking"></v-switch>
              </v-col>
              <v-col cols="12" sm="6" md="6">
                <v-switch v-model="filterConfig.IsCentral" label="Central Area"></v-switch>
              </v-col>
              <v-col col="12" sm="6" md="6">
                <div class="map-search-input">
                  <gmap-autocomplete :value="search" @place_changed="setPlace"></gmap-autocomplete>
                </div>
              </v-col>
              <v-col cols="12" sm="6" md="6">
                <v-select :items="vehType" v-model="filterConfig.VehType" label="Vehicle Type" solo></v-select>
              </v-col>
              <v-col cols="12" sm="6" md="6">
                <v-select
                  :items="agencyType"
                  v-model="filterConfig.AgencyType"
                  label="Agency Type"
                  solo
                ></v-select>
              </v-col>
              <v-col col="12" sm="6" md="6">
                <v-text-field v-model="filterConfig.Range" label="Range (m)"></v-text-field>
              </v-col>
            </v-list-item-content>
          </v-list-item>
          <v-card-actions>
            <div class="flex-grow-1"></div>
            <v-btn color="blue darken-1" text @click="filterCarparks">Filter</v-btn>
            <v-btn color="blue darken-1" text @click="clearFilter">Clear Filter</v-btn>
          </v-card-actions>
        </v-card>
      </v-menu>
    </v-card-text>
  </v-flex>
</template>
<script>
export default {
  data() {
    return {
      filterText: "",
      isFiltered: false,
      filterOpen: false,
      filterConfig: this.$store.getters.FILTER,
      vehType: ["Car", "Motorcycle", "Heavy Vehicle", "All"],
      agencyType: ["HDB", "URA", "LTA", "All"],
      description: "Singapore",
      longitude: 0,
      latitude: 0,
      search: ""
    };
  },
  watch: {
    filterText: function(filterText) {
      //When user type more than 2, filter the text
      //When user delete the input, return back the original list
      if (this.filterText.length >= 3 || this.filterText.length == 0) {
        this.$emit("change", this.filterText);
      }
    }
  },
  methods: {
    openFilter: function() {
      //Get the updated filterConfig settings
      this.filterConfig = this.$store.getters.FILTER;
    },
    filterCarparks: function() {
      //Update store filter
      //Pass the filter config to the parent component
      this.filterConfig.Latitude = this.latitude;
      this.filterConfig.Longitude = this.longitude;
      this.$store.dispatch("UPDATEFILTER", this.filterConfig).then(success => {
        this.$emit("clicked", this.filterConfig);
      });
    },
    clearFilter: function(event) {
      event.stopPropagation();
      this.search = null
      this.filterConfig = {
        //Set back default values
        IsAscending: true,
        IsElectronic: true,
        IsCentral: false,
        PageSize: 20,
        PageNumber: 1,
        VehType: "",
        AgencyType: "",
        Range: 1000,
        Latitude: 0,
        Longitude: 0
      };
    },
    setPlace: function(place) {
      this.latitude = place.geometry.location.lat();
      this.longitude = place.geometry.location.lng();
    }
  }
};
</script>
<style>
.filter {
  margin-top: -10px;
  margin-bottom: -45px;
}

.filterbox {
  margin-top: -15px !important;
  padding: 15px !important;
}

.filterheader {
  color: black;
  font-weight: 300;
  font-size: 18px;
}

.map-search-input {
  width: 100%;
}

.map-search-input input {
  width: 100%;
  padding: 8px;
  border-color: #37474f !important;
  box-shadow: inset 0 2px 5px rgba(0, 0, 0, 0.2);
}
</style>