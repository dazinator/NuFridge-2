var config = {
    providers: {
        google: {
            clientId: '503113912592-283tcms15a2mh5ksggtv60rmt7afnf3f.apps.googleusercontent.com'
        }
    },
    loginRedirect: "#/dashboard",
    baseUrl: "/",
    loginUrl: "/api/signin",
    loginRoute: '/signin',
    signupRedirect: "",
    httpInterceptor: true,
    authToken: "Token",
    logoutRedirect: "#/",
    profileUrl: "/api/account"
};

export default config;