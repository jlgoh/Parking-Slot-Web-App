/* eslint-disable no-unused-vars */
import axios from 'axios';
import store from '../store';

export default {
    state: {
        firstName: "",
        lastName: "",
        username: "",
        email: "",
        phoneNo: "",
        role: "",
        token: null,
        isLoggedIn: false,
        userid: "",
        favorites: [],
        filterConfig: {
            IsAscending: true,
            IsElectronic: true,
            IsCentral: false,
            PageSize: 20,
            PageNumber: 1,
            VehType: "",
            AgencyType: "",
            Range: 100,
            Latitude: 0,
            Longitude: 0
        }
    },
    getters: {
        FIRSTNAME: (state) => {
            return state.firstName;
        },
        LASTNAME: (state) => {
            return state.lastName;
        },
        USERNAME: state => {
            return state.username;
        },
        EMAIL: (state) => {
            return state.email;
        },
        PHONENO: (state) => {
            return state.phoneNo;
        },
        ROLE: (state) => {
            return state.role;
        },
        ISLOGGEDIN: (state) => {
            return state.isLoggedIn;
        },
        TOKEN: (state) => {
            return state.token;
        },
        USERID: (state) => {
            return state.userid;
        },
        FAVORITES: (state) => {
            return state.favorites;
        },
        FILTER: (state) => {
            return state.filterConfig
        }
    },
    mutations: {
        SETAUTHSTATUS(state, isLoggedIn) {
            state.isLoggedIn = isLoggedIn;
        },
        SETFIRSTNAME(state, firstName) {
            state.firstName = firstName;
        },
        SETLASTNAME(state, lastName) {
            state.lastName = lastName;
        },
        SETUSERNAME(state, username) {
            state.username = username;
        },
        SETEMAIL(state, email) {
            state.email = email;
        },
        SETPHONENO(state, phoneNo) {
            state.phoneNo = phoneNo;
        },
        SETROLE(state, role) {
            state.role = role;
        },
        SETTOKEN(state, token) {
            state.token = token;
        },
        SETUSERID(state, userid) {
            state.userid = userid;
        },
        ADDFAVORITES(state, carparkId) {
            state.favorites.push(carparkId)
        },
        CLEARFAVORITES(state) {
            state.favorites.length = 0
        },
        REMOVEFAVORITES(state, carparkId) {
            var index = state.favorites.indexOf(carparkId);
            if (index > -1) {
                state.favorites.splice(index, 1);
            }
        },
        SETFILTERCONFIG(state, filterConfig) {
            state.filterConfig = filterConfig;
        }
    },
    actions: {
        LOGIN: ({ commit }, payload) => {
            return new Promise((resolve, reject) => {
                /* POST to the Web API */
                axios.post(`https://parkingslotapi.azurewebsites.net/api/users/authenticate`, payload).then(({ data, status }) => {
                    if (status === 200) {
                        //Add user info to store
                        store.commit('SETUSERNAME', data.username);
                        store.commit('SETEMAIL', data.email);
                        store.commit('SETFIRSTNAME', data.firstName);
                        store.commit('SETLASTNAME', data.lastName);
                        store.commit('SETPHONENO', data.phoneNumber);
                        store.commit('SETROLE', data.role);
                        store.commit('SETTOKEN', data.token);
                        store.commit('SETUSERID', data.id);
                        store.commit('SETAUTHSTATUS', true);
                        resolve(true);
                    }
                }).catch(error => {
                    reject(error);
                })

            });
        },
        REGISTER: ({ commit }, payload) => {
            /* POST to the Web API */
            return new Promise((resolve, reject) => {
                axios
                    .post(`https://parkingslotapi.azurewebsites.net/api/users`, payload)
                    .then(({ data, status }) => {
                        if (status === 200) {
                            resolve(data);
                        }
                    })
                    .catch(error => {
                        //console.log(error.response.data);
                        //console.log(error.response.status);
                        reject(error);
                    });
            });
        },
        CHECKTOKEN: ({ commit }, token) => {
            return new Promise((resolve, reject) => {
                axios
                    .post("https://parkingslotapi.azurewebsites.net/api/users/checktoken", {
                        Token: token
                    })
                    .then(function (response) {
                        resolve(true);
                    }).catch(error => {
                        reject(error);
                    });
            })
        },
        UPDATE: ({ commit }, payload) => {
            return new Promise((resolve, reject) => {
                /* PUT to the Web API */
                let userid = store.getters.USERID;
                //Handle for admin side editing of user information
                if (payload.UserId != undefined) {
                    userid = payload.UserId;
                }
                axios.put(`https://parkingslotapi.azurewebsites.net/api/users/${userid}`, payload, { headers: { Authorization: "Bearer " + store.getters.TOKEN } }).then(({ data, status }) => {
                    if (status === 204) {
                        //Update user info to store
                        store.commit('SETFIRSTNAME', payload.FirstName);
                        store.commit('SETLASTNAME', payload.LastName);
                        store.commit('SETEMAIL', payload.Email);
                        store.commit('SETPHONENO', payload.PhoneNumber);
                        resolve(true);
                    }
                }).catch(error => {
                    reject(error);
                })
            });
        },
        CHANGEPASSWORD: ({ commit }, payload) => {
            return new Promise((resolve, reject) => {
                axios.post("https://parkingslotapi.azurewebsites.net/api/users/resetpassword", payload).then(function (response) {
                    resolve(true);
                }).catch(error => {
                    reject(error);
                })
            });
        },
        SETNEWPASSWORD: ({ commit }, Update) => {
            return new Promise((resolve, reject) => {
                axios.post("https://parkingslotapi.azurewebsites.net/api/users/confirmpassword", Update).then(function (response) {
                    resolve(true);
                }).catch(error => {
                    reject(error);
                })
            });
        },
        GETFAVORITES: ({ commit }) => {
            return new Promise((resolve, reject) => {
                axios
                    .get(
                        "https://parkingslotapi.azurewebsites.net/api/users/" +
                        store.getters.USERID +
                        "/favorites"
                    )
                    .then(function (response) {
                        store.commit("CLEARFAVORITES"); //Clear in store
                        if (response.data.length != 0) {
                            for (var i = 0; i < response.data.length; i++) {
                                //push each carpark id into the array
                                store.commit('ADDFAVORITES', response.data[i].carparkId);
                            }
                        }
                        resolve(response);
                    }).catch(error => {
                        reject(error);
                    })
            });
        },
        ADDFAVORITES: ({ commit }, carparkId) => {
            return new Promise((resolve, reject) => {
                axios.post(
                    "https://parkingslotapi.azurewebsites.net/api/users/" + store.getters.USERID + "/favorites",
                    {
                        CarparkId: carparkId
                    }
                )
                    .then(function () {
                        store.commit('ADDFAVORITES', carparkId);
                        resolve(true);
                    }).catch(error => {
                        reject(error);
                    });
            })
        },
        DELETEFAVORITE: ({ commit }, carparkId) => {
            return new Promise((resolve, reject) => {
                axios.delete("https://parkingslotapi.azurewebsites.net/api/users/" +
                    store.getters.USERID +
                    "/favorites/" + carparkId).then(function (response) {
                        store.commit('REMOVEFAVORITES', carparkId);
                        resolve(response);
                    }).catch(error => {
                        reject(error);
                    });
            });
        },
        UPDATEFILTER: ({ commit }, filterConfig) => {
            return new Promise((resolve, reject) => {
                store.commit('SETFILTERCONFIG', filterConfig);
                resolve(true);
            })
        },
        CREATEFEEDBACK: ({ commit }, feedback) => {
            return new Promise((resolve, reject) => {
                axios.post("https://parkingslotapi.azurewebsites.net/api/feedbacks" + "/user/" + store.getters.USERID, feedback).then(function (response) {
                    resolve(response);
                }).catch(error => {
                    reject(error);
                });
            });
        }
    }
};