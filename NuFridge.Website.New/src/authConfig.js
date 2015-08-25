var configForDevelopment = {
    providers: {
        google: {
            clientId: '503113912592-283tcms15a2mh5ksggtv60rmt7afnf3f.apps.googleusercontent.com'
        }
    },
    loginRedirect: "/",
    loginUrl: "/api/signin",
    loginRoute: '/signin',
    signupRedirect: "/",
    httpInterceptor: true,
    authToken: "Token",
    logoutRedirect: "/signin",
    profileUrl: "/api/account"
};

var configForProduction = {
	providers: {
		google: {
		    clientId: '503113912592-283tcms15a2mh5ksggtv60rmt7afnf3f.apps.googleusercontent.com'
		} 
	}
};
var config ;
if (window.location.hostname==='localhost') {
	config = configForDevelopment;
}
else{
	config = configForProduction;
}



export default config;
