<template>
  <v-content style="margin-top:0px" v-resize="onResize" column>
    <v-alert type="success" style="border-radius:0px;" :value="alertSuccess">{{successmsg}}</v-alert>
    <v-data-table
      :headers="headers"
      :items="feedbacks"
      item-key="id"
      sort-by="topic"
      :options.sync="pagination"
      :server-items-length="totalFeedbacks"
      :loading="loading"
      :hide-default-header="isMobile"
      :class="{mobile: isMobile}"
      v-model="selected"
    >
      <template v-slot:top>
        <v-toolbar flat>
          <v-dialog v-model="showCreate" max-width="1000px">
            <template v-slot:activator="{ on }">
              <v-spacer></v-spacer>
              <v-btn color="primary" dark class="ml-5 mb-2 mt-3" to="/admin">Manage Users</v-btn>
            </template>
          </v-dialog>
        </v-toolbar>

        <v-dialog v-model="showFeedback" max-width="1000px">
          <v-card>
            <v-card-title>
              <span class="headline">Manage User Feedbacks</span>
              <v-chip
                v-if="viewFeedback.isResolved == 'True'"
                class="ma-2"
                color="green"
                outlined
              >Resolved</v-chip>
              <v-chip
                v-if="viewFeedback.isResolved == 'False'"
                class="ma-2"
                color="orange"
                outlined
              >Pending</v-chip>
            </v-card-title>
            <v-divider></v-divider>
            <v-card-text>
              <v-container>
                <v-row>
                  <v-col cols="12">
                    <v-text-field v-model="viewFeedback.topic" label="Topic" outlined readonly></v-text-field>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col cols="12">
                    <v-textarea
                      v-model="viewFeedback.description"
                      auto-grow
                      label="Description"
                      outlined
                      readonly
                    ></v-textarea>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col cols="12">
                    <v-textarea
                      v-model="viewFeedback.comments"
                      auto-grow
                      label="Comments"
                      outlined
                      required
                    ></v-textarea>
                  </v-col>
                </v-row>
                <v-card-actions>
                  <div class="flex-grow-1"></div>
                  <v-btn
                    color="green darken-1"
                    text
                    v-if="viewFeedback.isResolved == 'False' || viewFeedback.isResolved == false"
                    @click="update(viewFeedback, true)"
                  >Resolve</v-btn>
                  <v-btn
                    color="red darken-1"
                    text
                    v-if="viewFeedback.isResolved == 'True' || viewFeedback.isResolved == true"
                    @click="update(viewFeedback, false)"
                  >Revoke</v-btn>
                </v-card-actions>
              </v-container>
            </v-card-text>
          </v-card>
        </v-dialog>
      </template>

      <template v-slot:body="{ items }">
        <tbody>
          <tr v-for="item in items" :key="item.id">
            <template v-if="!isMobile">
              <td>{{item.topic}}</td>
              <td>{{item.description}}</td>
              <td>{{item.comments}}</td>
              <td>
                <v-layout align-center justify-center>
                  <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-icon
                      size="30"
                      color="green darken-1"
                      v-if="item.isResolved == 'True' || item.isResolved == true"
                    >mdi-checkbox-marked-circle</v-icon>
                    <v-icon
                      size="30"
                      color="orange darken-1"
                      v-if="item.isResolved == 'False' || item.isResolved == false"
                    >mdi-alert-circle-outline</v-icon>
                    <v-btn color="blue darken-1" text @click.stop="viewItem(item)">View</v-btn>
                    <v-btn color="red darken-1" text @click.stop="deleteItem(item)">Delete</v-btn>
                  </v-card-actions>
                </v-layout>
              </td>
            </template>
            <template v-else>
              <td>
                <ul class="flex-content" style="list-style-type: none;">
                  <li class="flex-item" data-label="Topic">{{ item.topic }}</li>
                  <li class="flex-item" data-label="Description">{{ item.description }}</li>
                  <li class="flex-item" data-label="Comments">{{ item.comments }}</li>
                  <li class="flex-item" data-label="Status">
                    <v-card-actions>
                      <v-btn color="blue darken-1" text @click.stop="viewItem(item)">View</v-btn>
                      <v-btn color="red darken-1" text @click.stop="deleteItem(item)">Delete</v-btn>
                    </v-card-actions>
                  </li>
                </ul>
              </td>
            </template>
          </tr>
        </tbody>
      </template>
    </v-data-table>
  </v-content>
</template>

<script>
export default {
  data: () => ({
    selected: [],
    showFeedback: false,
    showCreate: false,
    showEdit: false,
    headers: [
      {
        text: "Topic",
        value: "topic",
        width: 200
      },
      { text: "Description", value: "description" },
      { text: "Comments", value: "comments" },
      {
        text: "Actions",
        value: "actions",
        align: "center",
        filterable: false,
        sortable: false,
        width: 150
      }
    ],
    feedbacks: [],
    viewFeedback: {},
    totalFeedbacks: 0,
    pagination: {},
    PageNumber: 1,
    loading: true,
    sortingName: "topic",
    isAscending: true,
    isMobile: false,
    alertSuccess: false,
    successmsg: ""
  }),
  watch: {
    options: {
      handler() {
        this.fetchFeedbacks();
      },
      deep: true
    }
  },
  computed: {
    options(nv) {
      return {
        ...this.pagination
      };
    }
  },
  methods: {
    onResize() {
      if (window.innerWidth < 769) this.isMobile = true;
      else this.isMobile = false;
    },
    fetchFeedbacks() {
      let cur = this;
      this.loading = true;
      const { sortBy, sortDesc, itemsPerPage } = this.pagination;
      this.PageNumber = this.pagination.page;

      this.sortingName = sortBy[0];
      this.isAscending = sortDesc[0];

      if (this.isAscending != true && this.isAscending != undefined) {
        this.sortingName = this.sortingName + " desc";
      }

      let config = {
        headers: { Authorization: "Bearer " + cur.$store.getters.TOKEN },
        params: {
          PageNumber: this.PageNumber,
          OrderBy: this.sortingName
        }
      };
      this.axios
        .get("https://parkingslotapi.azurewebsites.net/api/feedbacks", config)
        .then(function(response) {
          cur.feedbacks = response.data;
          cur.totalFeedbacks = response.data.length;
          setTimeout(() => {
            cur.loading = false;
          }, 500);
        });
    },
    viewItem(item) {
      this.viewFeedback = Object.assign({}, item);
      this.showFeedback = true;
    },
    getFeedbackIndex(feedbackid) {
      let userIndex = this.feedbacks
        .map(feedback => {
          return feedback.id;
        })
        .indexOf(feedbackid);
      return userIndex;
    },
    update(item, value) {
      item.isResolved = value;
      if (value == true) {
        item.isResolved = true;
      } else {
        item.isResolved = false;
      }

      let cur = this;
      cur.showFeedback = false;
      this.axios
        .put(
          "https://parkingslotapi.azurewebsites.net/api/feedbacks/" + item.id,
          {
            isResolved: item.isResolved,
            comments: item.comments
          }
        )
        .then(function(response) {
          const index = cur.getFeedbackIndex(item.id);
          cur.$set(cur.feedbacks, index, item);
          cur.showFeedback = false;
          cur.alertSuccess = true;
          cur.alertError = false;
          cur.successmsg = "Feedback have been successfully updated!";
        });
    },
    deleteItem(item) {
      var result = confirm("Delete Feedback?");
      if (result == true) {
        let cur = this;
        this.axios
          .delete(
            "https://parkingslotapi.azurewebsites.net/api/feedbacks/" +
              item.id +
              "/user/" +
              item.userId,
            {
              headers: { Authorization: "Bearer " + cur.$store.getters.TOKEN }
            }
          )
          .then(function(response) {
            const index = cur.getFeedbackIndex(item.id);
            if (~index) {
              cur.feedbacks.splice(index, 1);
              cur.totalFeedbacks = cur.totalFeedbacks - 1;
            }
            cur.alertSuccess = true;
            cur.alertError = false;
            cur.successmsg = "Feedback have been successfully deleted!";
          });
      }
    }
  }
};
</script>
<style>
@import url("../../assets/table.css");
</style>