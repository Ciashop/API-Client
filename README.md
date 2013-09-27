#API-Client
==========
Este projeto contém uma biblioteca para ajudar desenvolvedores na plataforma .Net criarem aplicações integradas à plataforma de lojas virtuais Ciashop Framework. São cobertos os processos de Autenticação e Acesso aos recursos disponibilizados em nossa API.
Para compreender melhor o processo de Autenticação e como deve ser feito as chamados aos métodos, recomendamos que leia a documentação disponível em nosso [Wiki] (http://wiki.ciashop.com.br/desenvolvedores/apis).

Atualmente o Client está desenvolvido somente em C#, mas isso não impede que seja consumida por linguagens que sejam compatíveis com Json. Pretendemos desenvolver nosso APIClient em outras linguagens no futuro, caso tenha interesse em traduzi-lo entre em contato conosco e vincularemos com nossa documentação.

Se você é desenvolvedor em outra linguagem ou plataforma, consulte também nosso Wiki para ter informações sobre como utilizar nossa API sem um client.


##Autor
Ciashop 

##Requisitos
* .Net 4.0
* Visual Studio 2010
* Chaves (ClientID e SecretKey) fornecidas pela Ciashop, através da nossa equipe de [Suporte](http://www.ciashop.com.br/contato/).

##Instalação
Faça o download do código fonte e acrescente ao projeto da sua solução.

##Ciashop API Authorization
A API Ciashop utiliza OAuth 2.0 como seu mecanismo de autenticação e a  classe ApiAuthorizer provê os métodos para autenticar sua aplicação.

O processo de autenticação coberto por esta classe está detalhado em nosso [Wiki/Autenticação](http://wiki.ciashop.com.br/desenvolvedores/apis/autenticacao/), em especial os processos identificados pelos itens 1 e 3 do Fluxo de Autenticação.

###Usando APIAuthorizer
Na primeira fase da autenticação o usuário deve ser redirecionado para a Url de autorização. O método GetAuthorization devolve a Url pronta para o redirect. Descrito no item 1 do [Wiki/Autenticação](http://wiki.ciashop.com.br/desenvolvedores/apis/autenticacao/#iniciando_autenticacao).

```csharp
APIAuthorizer authorizer = new APIAuthorizer(ConfigurationManager.AppSettings["clientId"],
                ConfigurationManager.AppSettings["secretKey"],
                ConfigurationManager.AppSettings["storeUrl"],
                ConfigurationManager.AppSettings["scope"]
                );

    // get the Authorization URL and redirect the user
    var authUrl = authorizer. GetAuthorization();
    Redirect(authUrl);
```    
Na segunda fase da autenticação, o método  AuthorizeClient deve ser invocado com o Token Temporário devolvido pela loja , descrito no item 2 do [Wiki/Autenticação](http://wiki.ciashop.com.br/desenvolvedores/apis/autenticacao/#token_temporario). Este método cuidará de todo processo para obtenção do Token Definitivo 
e retornará um objeto AuthState, que será utilizado pelo Client em toda requisição da API.
```csharp
AuthState authState = authorizer.AuthorizeClient(code);
    if (authState != null && authState.AccessToken != null)
    {
        // store the auth state in the session or DB to be used for all API calls for the specified shop
    }
```

##Uso do API Client
Para conseguir utilizar o API Client da Ciashop é preciso conhecer a nossa documentação: [API](http://wiki.ciashop.com.br/desenvolvedores/apis). Projetamos a classe APIClient de forma a facilitar as chamadas de URLs da API e o formato de envio dos dados. 
Após utilizar a classe APIAuthorizer e obter a autorização, já poderá realizar as chamadas dos outros recursos disponíveis na API.

Você pode utilizar a classe APIClient para execução dos métodos GET, PUT, POST e DELETE.

###Usando a classe APIClient
Listar todos os departamentos (Get Departaments)
  
```csharp
 //Get all Departments from the API. (.NET 4.0)
 APIClient objClient = new APIClient(authState);
 
 //This object contains all definition of headers and data content
 var response = objClient.Get("departments");
 
// the dynamic object will have all the fields just like in the API Docs
foreach(var product in response.Content)
{
	Console.Write(product.title);
}
```
Criar um novo Departamento (Post Departments)
```csharp   
   //Post Departments. (.NET 4.0)

 APIClient objClient = new APIClient(authState);
 
 dynamic department = new
						{
							name = "Department",
							description = "Description",
							sortOrder = "1",
							visible = true
						};
						
var response = objClient.Post("departments", department);

if(response.StatusCode == 200)
	Console.Write("Success");
```
Atualizar um Departamento (Put Departments)
```csharp   
   //Put Departments. (.NET 4.0)

 APIClient objClient = new APIClient(authState);
 
 dynamic department = new
						{
							name = "Department",
							description = "Description",
							sortOrder = "1",
							visible = true
						};
						
var response = objClient.Put("departments", department);

if(response.StatusCode == 201)
	Console.Write("Success");
```
Deletar um Departamento (Delete Departments)
```csharp   
  //Delete Departments (.NET 4.0)

APIClient objClient = new APIClient(authState);

var response = objClient.Delete("departments/99");
if(response.StatusCode == 200)
	Console.Write("Success");
```
Recuperar  Erros.

Acesse nosso Wiki para visualizar os [Erros Gerais da API](http://wiki.ciashop.com.br/desenvolvedores/apis/erros-gerais-da-api/).
```csharp   
 APIClient objClient = new APIClient(authState);

var response = objClient.Get("/departments/Test");

if(response.StatusCode != 200 && response.Error.Message)
	Console.Write("Error description: " + response.Error.Message);
```
Usar Headers.

O Exemplo abaixo utiliza o Header x-apilimit-remaining para exibir os limites de Chamadas, para sobre os Headers disponíveis consulte as [Definições Gerais da Api](http://wiki.ciashop.com.br/desenvolvedores/apis/definicoes-gerais/#headers).
```csharp   
 //Using specific headers(.NET 4.0)
APIClient objClient = new APIClient(authState);

var response = objClient.Get("/departments");

Console.Write("Has more Registers: " + response.HasMore);
Console.Write("My api call limit: " + response.ApiLimit);
Console.Write("My api calls limit Remaining: " + response.ApiLimitRemaining);
```
