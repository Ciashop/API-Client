#API-Client
==========

##Autor
Ciashop 

##Requisitos
* .Net 4.0, a pasta DotNet4.0 contém uma solução usando o Visual Studio 2010.

##Instalação
Faça o download do código fonte e acrescente ao projeto da sua solução.

##Ciashop API Authorization
Para compreender melhor o API Client da Ciashop e como deve ser feito as chamados aos métodos, recomendamos que leia a documentação disponível em nosso [Wiki] (http://wiki.ciashop.com.br/desenvolvedores/apis).

###Ciashop APIAuthorizer
Abaixo está a classe que deve estar no seu projeto e realiza a autorização do seu app.

```csharp
/// <summary>
        /// Creates an instance of this class in order to obtain the authorization
        /// from the customer to make api calls on their behalf
        /// </summary>
        /// <param name="clientId">the unique api key of your client</param>
        /// <param name="secretKey">the unique secret key of your client</param>
        /// <param name="storeUrl">url of your client store</param>
        /// <param name="scope">the scopes required for authorization</param>
        public APIAuthorizer(string clientId, string secretKey, string storeUrl, string scope)
        {


/// <summary>
        /// Creates an instance of this class in order to obtain the authorization
        /// from the customer to make api calls on their behalf
        /// </summary>
        /// <param name="clientId">the unique api key of your client</param>
        /// <param name="secretKey">the unique secret key of your client</param>
        /// <param name="storeUrl">url of your client store</param>
        /// <param name="scope">the scopes required for authorization</param>
        public APIAuthorizer(string clientId, string secretKey, string storeUrl, string scope)


/// <summary>
        /// Get the URL required by you to redirect the User to in which they will be
        /// presented with the ability to grant access to app with the specified scope
        /// </summary>
        /// <returns>url to make a a redirect call</returns>
        public string GetAuthorization()


/// <summary>
        /// After the shop owner has authorized your app, shop will give you a code.
        /// Use this code to get your authorization state that you will use to make API calls
        /// </summary>
        /// <param name="code">a code given to you by shop</param>
        /// <returns>Authorization state needed by the API client to make API calls</returns>
        public AuthState AuthorizeClient(string code)
}
```
###Usando APIAuthorizer
Abaixo segue um exemplo que mostra como a classe APIAuthorizer deverá ser usada.

```csharp
APIAuthorizer authorizer = new APIAuthorizer(ConfigurationManager.AppSettings["clientId"],
                ConfigurationManager.AppSettings["secretKey"],
                ConfigurationManager.AppSettings["storeUrl"],
                ConfigurationManager.AppSettings["scope"]
                );

    // get the Authorization URL and redirect the user
    var authUrl = authorizer. GetAuthorization();
    Redirect(authUrl);
    // Meanwhile the User is click "yes" to authorize your app for the specified scope. 

    // Once this click, yes or no, they are redirected back to the return URL

    // Handle response callback and recover code value
    // if developer the APIAuthorizer object so get the authorization state
    AuthState authState = authorizer.AuthorizeClient(code);
    if (authState != null && authState.AccessToken != null)
    {
        // store the auth state in the session or DB to be used for all API calls for the specified shop
    }
```    
 
##Uso do API Client
Para conseguir utilizar o API Client da Ciashop é preciso conhecer a nossa documentação: [API](http://wiki.ciashop.com.br/desenvolvedores/apis). Projetamos a classe APIClient de forma a facilitar as chamadas de URLs da API e o formato de envio dos dados. 
Após utilizar a classe APIAuthorizer e obter a autorização, já poderá realizar as chamadas dos outros métodos da API.

###Usando a classe APIClient
Get de todos os departamentos.
  
```csharp
  APIClient api = new APIClient(authState);

    // by default JSON string is returned
    object data = api.Get("/admin/products.json");

    // use your favorite JSON library to decode the string into a C# object
```
Cadastrar um Departamento.
```csharp   
    APIClient api = new APIClient(authState);

    // Manually construct a JSON string or in some other way
    // Ugly
    string dataToUpdate = 
        "{" +
            "\"product\": {" +
                "\"title\": \"Burton Custom Freestlye 151\"," +
                "\"body_html\": \"<strong>Good snowboard!</strong>\"" +
            "}" +
        "}";

    string createProductResponse = api.Post("/admin/products.json");
```
Atualizar um Departamento.
```csharp   
    // pass the supplied JSON Data Translator
    var api = new APIClient(authState, new JsonDataTranslator());

    // use dynamics to create the object
    // a lot nicer that the previous way
    dynamic newProduct = new
    { 
        products = new { 
            title       = "Burton Custom Freestlye 151", 
            body_html   = "<strong>Good snowboard!</strong>"
        } 
    };
    dynamic createProductResponse = api.Post("/admin/products.json", newProduct);

```
Deletar um Departamento.
```csharp   
    // id of the product you wish to delete
    int id = 123;
    var api = new APIClient(authState, new JsonDataTranslator());
    api.Delete(String.Format("/admin/products/{0}.json", id));
```


