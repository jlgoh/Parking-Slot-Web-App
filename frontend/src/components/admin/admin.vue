<template>
  <v-content style="margin-top:0px" v-resize="onResize" column>
    <v-alert type="error" style="border-radius:0px;" :value="alertError">{{ errmsg}}</v-alert>
    <v-alert
      type="success"
      style="border-radius:0px;"
      :value="alertSuccess"
    >{{successmsg}}</v-alert>
    <v-data-table
      :headers="headers"
      :items="users"
      item-key="id"
      sort-by="username"
      :options.sync="pagination"
      :server-items-length="totalUsers"
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
              <v-btn color="teal" dark class="ml-5 mb-2 mt-3" to="/feedback">Manage Feedbacks</v-btn>
              <v-btn
                color="primary"
                dark
                class="ml-5 mb-2 mt-3"
                v-on="on"
                @click="createUser"
              >Create User</v-btn>
            </template>
            <v-card>
              <v-card-title>
                <span class="headline">Create User</span>
              </v-card-title>
              <v-divider></v-divider>
              <v-card-text class="mt-2">
                <v-container>
                  <v-form ref="createform">
                    <v-row>
                      <v-col cols="12" sm="6" md="6">
                        <v-text-field v-model="newUser.username" label="Username" required></v-text-field>
                      </v-col>
                      <v-col cols="12" sm="6" md="6">
                        <v-text-field v-model="newUser.phoneNumber" label="Phone Number" required></v-text-field>
                      </v-col>
                    </v-row>
                    <v-row>
                      <v-col cols="12">
                        <v-text-field v-model="newUser.email" label="Email" required></v-text-field>
                      </v-col>
                    </v-row>
                    <v-row>
                      <v-col cols="12" sm="6" md="6">
                        <v-text-field v-model="newUser.firstName" label="First Name" required></v-text-field>
                      </v-col>
                      <v-col cols="12" sm="6" md="6">
                        <v-text-field v-model="newUser.lastName" label="Last Name" required></v-text-field>
                      </v-col>
                    </v-row>
                    <v-row>
                      <v-col cols="12" sm="6" md="6">
                        <v-text-field v-model="newUser.password" label="Password" type="password" required></v-text-field>
                      </v-col>
                      <v-col cols="12" sm="6" md="6">
                        <v-text-field
                          v-model="newUser.confirmPassword"
                          label="Confirm Password"
                                      type="password"
                          required
                        ></v-text-field>
                      </v-col>
                    </v-row>
                  </v-form>
                </v-container>
              </v-card-text>
              <v-card-actions>
                <div class="flex-grow-1"></div>
                <v-btn color="blue darken-1" text @click="cancelCreate">Cancel</v-btn>
                <v-btn color="blue darken-1" text @click="submitUser">Create</v-btn>
              </v-card-actions>
            </v-card>
          </v-dialog>
        </v-toolbar>

        <v-dialog v-model="showEdit" max-width="1000px">
          <v-card>
            <v-card-title>
              <span class="headline">Edit {{editUserInfo.username}}</span>
            </v-card-title>
            <v-divider></v-divider>
            <v-card-text class="mt-2">
              <v-container>
                <v-form ref="editform">
                  <v-row>
                    <v-col cols="12" sm="6" md="6">
                      <v-text-field
                        v-model="editUserInfo.phoneNumber"
                        label="Phone Number"
                        required
                      ></v-text-field>
                    </v-col>
                    <v-col cols="12" sm="6" md="6">
                      <v-text-field v-model="editUserInfo.email" label="Email"></v-text-field>
                    </v-col>
                  </v-row>
                  <v-row>
                    <v-col cols="12" sm="6" md="6">
                      <v-text-field v-model="editUserInfo.firstName" label="First Name" required></v-text-field>
                    </v-col>
                    <v-col cols="12" sm="6" md="6">
                      <v-text-field v-model="editUserInfo.lastName" label="Last Name" required></v-text-field>
                    </v-col>
                  </v-row>
                </v-form>
              </v-container>
            </v-card-text>

            <v-card-actions>
              <div class="flex-grow-1"></div>
              <v-btn color="blue darken-1" text @click="cancelEdit">Cancel</v-btn>
              <v-btn color="blue darken-1" text @click="submitEdit">Submit</v-btn>
            </v-card-actions>
          </v-card>
        </v-dialog>

        <v-dialog v-model="showUser" max-width="1000px">
          <v-card>
            <v-card-title>
              <span class="headline">{{ viewUser.username }}</span>
            </v-card-title>
            <v-divider></v-divider>
            <v-card-text>
              <v-container>
                <v-row>
                  <v-col cols="12" sm="6" md="6">
                    <v-text-field v-model="viewUser.firstName" label="First Name" outlined readonly></v-text-field>
                  </v-col>
                  <v-col cols="12" sm="6" md="6">
                    <v-text-field v-model="viewUser.lastName" label="Last Name" outlined readonly></v-text-field>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col cols="12" sm="6" md="6">
                    <v-text-field
                      v-model="viewUser.phoneNumber"
                      label="Phone Number"
                      outlined
                      readonly
                    ></v-text-field>
                  </v-col>
                  <v-col cols="12" sm="6" md="6">
                    <v-text-field v-model="viewUser.email" label="Email" outlined readonly></v-text-field>
                  </v-col>
                </v-row>
                <v-card-actions>
                  <div class="flex-grow-1"></div>
                </v-card-actions>
                <v-card-actions>
                  <div class="flex-grow-1"></div>
                </v-card-actions>
              </v-container>
            </v-card-text>
          </v-card>
        </v-dialog>
      </template>

      <template v-slot:body="{ items }">
        <tbody>
          <tr v-for="item in items" :key="item._id">
            <template v-if="!isMobile">
              <td>{{item.username}}</td>
              <td>{{item.firstName}}</td>
              <td>{{item.lastName}}</td>
              <td>{{item.email}}</td>
              <td>{{item.phoneNumber}}</td>
              <td>
                <v-layout align-center justify-center>
                  <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="blue darken-1" text @click.stop="viewItem(item)">View</v-btn>
                    <v-btn color="orange darken-1" text @click.stop="editUser(item)">Edit</v-btn>
                    <v-btn color="red darken-1" text @click.stop="deleteUser(item)">Delete</v-btn>
                  </v-card-actions>
                </v-layout>
              </td>
            </template>
            <template v-else>
              <td>
                <ul class="flex-content" style="list-style-type: none;">
                  <li class="flex-item" data-label="Username">{{ item.username }}</li>
                  <li class="flex-item" data-label="First Name">{{ item.firstName }}</li>
                  <li class="flex-item" data-label="Last Name">{{ item.lastName }}</li>
                  <li class="flex-item" data-label="Email">{{ item.email }}</li>
                  <li class="flex-item" data-label="Phone Number">{{ item.phoneNumber }}</li>
                  <li class="flex-item" data-label="Status">
                    <v-card-actions>
                      <v-btn color="blue darken-1" text @click.stop="viewItem(item)">View</v-btn>
                      <v-btn color="orange darken-1" text @click.stop="editUser(item)">Edit</v-btn>
                      <v-btn color="red darken-1" text @click.stop="deleteUser(item)">Delete</v-btn>
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
    showUser: false,
    showCreate: false,
    showEdit: false,
    headers: [
      {
        text: "Username",
        value: "username"
      },
      { text: "First Name", value: "firstName" },
      { text: "Last Name", value: "lastName" },
      { text: "Email", value: "email" },
      {
        text: "Phone Number",
        value: "phoneNumber",
        filterable: false,
        sortable: false
      },
      {
        text: "Actions",
        value: "actions",
        align: "center",
        filterable: false,
        sortable: false,
        width: 150
      }
    ],
    users: [],
    viewUser: {},
    editUserInfo: {},
    newUser: {},
    totalUsers: 0,
    pagination: {},
    PageNumber: 1,
    loading: true,
    sortingName: "username",
    isMobile: false,
    isAscending: true,
    alertError: false,
    alertSuccess: false,
    errmsg: "",
    successmsg: ""
  }),
  watch: {
    options: {
      handler() {
        this.fetchUsers();
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
    fetchUsers() {
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
        params: {
          PageNumber: this.PageNumber,
          OrderBy: this.sortingName
        }
      };
      this.axios
        .get("https://parkingslotapi.azurewebsites.net/api/users", config)
        .then(function(response) {
          cur.users = response.data.userDtos;
          cur.totalUsers = response.data.totalCount;
          setTimeout(() => {
            cur.loading = false;
          }, 500);
        });
    },
    viewItem(item) {
      this.viewUser = Object.assign({}, item);
      this.showUser = true;
    },
    getUserIndex(userid) {
      let userIndex = this.users
        .map(user => {
          return user.id;
        })
        .indexOf(userid);
      return userIndex;
    },
    createUser() {
      this.showCreate = true;
    },
    submitUser() {
      var newUser = this.newUser;
      if (this.$refs.createform.validate()) {
        if (this.newUser.Password != this.newUser.ConfirmPassword) {
          alert("Password do not match");
        } else {
          this.$store
            .dispatch("REGISTER", this.newUser)
            .then(response => {
              var username = this.newUser.username;
              this.users.push(response);
              this.newUser = {};
              this.showCreate = false;
              this.alertSuccess = true;
              this.alertError = false;
              this.successmsg =
                "User " + username + " have been successfully created!";
            })
            .catch(error => {
              console.log(error);
              this.alertSuccess = false;
              this.alertError = true;
              this.errmsg = "Bad request sent to the server";
            });
        }
      }
    },
    cancelCreate() {
      for (let keys in this.newUser) delete this.newUser[keys];
      this.showCreate = false;
    },
    cancelEdit() {
      this.showEdit = false;
    },
    editUser(item) {
      this.editUserInfo = Object.assign({}, item);
      this.showEdit = true;
    },
    submitEdit() {
      if (this.$refs.editform.validate()) {
        var cur = this;
        this.$store
          .dispatch("UPDATE", {
            FirstName: this.editUserInfo.firstName,
            LastName: this.editUserInfo.lastName,
            Email: this.editUserInfo.email,
            PhoneNumber: this.editUserInfo.phoneNumber,
            UserId: this.editUserInfo.id
          })
          .then(success => {
            cur.showEdit = false;
            var index = this.getUserIndex(cur.editUserInfo.id);
            this.$set(this.users, index, this.editUserInfo);
            this.alertSuccess = true;
            this.alertError = false;
            this.successmsg = "Profile have been successfully updated!";
          })
          .catch(error => {
            console.log(error);
            this.alertSuccess = false;
            this.alertError = true;
            this.errmsg = "Bad request sent to the server";
          });
      } else {
        alert("Form not validated");
      }
    },
    deleteUser(item) {
      var result = confirm("Delete " + item.username + "?");
      if (result == true) {
        let cur = this;
        this.axios
          .delete(
            "https://parkingslotapi.azurewebsites.net/api/users/" + item.id,
            {
              headers: { Authorization: "Bearer " + cur.$store.getters.TOKEN }
            }
          )
          .then(function(response) {
            const index = cur.getUserIndex(item.id);
            if (~index) {
              cur.users.splice(index, 1);
              cur.totalUsers = cur.totalUsers - 1;
            }
            cur.alertSuccess = true;
            cur.alertError = false;
            cur.successmsg = "User have been successfully deleted!";
          });
      }
    }
  }
};
</script>
<style>
@import url("../../assets/table.css");
</style>