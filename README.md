# Directory.Packages.props - Centralizando versões dos pacotes Nuget

## Contexto
O processo de gerenciar as versões dos pacotes utilizados pela aplicação pode se tornar complexo quando há muitas aplicações que dependem dos mesmos pacotes e, em alguns casos, que também precisem utilizar as mesmas versões.
Um exemplo disso é quando se trabalha com vários microservices e, com isso, cria-se um repositório responsável por criar pacotes com códigos em comum e/ou precisem ser compartilhados entre todos os MS. 
Nesse cenário, quando ocorre uma nova geração de versão desses pacotes comuns, é necessário alterar todos os MS impactados para que passem a utilizar essa última versão, trabalho que pode ser repetitivo e demorado.

## Uma alternativa para o problema
Uma alternativa para contornar o problema acima, seria criar um arquivo que tem como objetivo centralizar o gerenciar versões dos pacotes utilizados pela aplicação. Com isso, ao invés de deixar explícito em cada aplicação a versão a ser utilizada para um determinado pacote, essa versão é definada no arquivo de configuração.
Em aplicações .NET, esse arquivo de configuração é definido como `Directory.Packages.props`. Basta criar um arquivo na raiz da solution (ou na raiz da cada projeto dentro da solution, o projeto irá utilizar o arquivo mais próximo dele).

![image](https://github.com/user-attachments/assets/4b5a1cef-c0e6-4a7d-8db4-0eb703178274)

Nesse arquivo, é definido quais os pacotes que serão utilizados e, respectivamente, qual será a versão de cada um dos pacotes.

```xml
<Project>
  <ItemGroup>
    <PackageVersion Include="Microsoft.AspNetCore.OpenApi" Version="8.0.8"/>
    <PackageVersion Include="Newtonsoft.Json" Version="13.0.3"/>
    <PackageVersion Include="Swashbuckle.AspNetCore" Version="6.7.3"/>
  </ItemGroup>
</Project>
```

Com isso, em cada um dos projetos (`.csprog`) presente na solution, não precisará mais especificar qual é a versão de cada pacote utilizado, basta agora somente indicar qual o pacote.
O trecho `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>` é opcional, mas deixa explícito que está sendo utilizado um gerenciado de versões de pacotes.
Caso queira desabilitar essa referência ao gerenciar de pacotes, basta colocar a flag como `false`, e com isso será exigido que informe a versão de cada um dos pacotes presente.

![image](https://github.com/user-attachments/assets/2867bb11-5398-41fc-b711-a61f12da0bef)

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi"/>
    <PackageReference Include="Newtonsoft.Json"/>
    <PackageReference Include="Swashbuckle.AspNetCore"/>
  </ItemGroup>

</Project>
```

## Cenário de uso para otimizar o processo de desenvolvimento
Como dito anteriormente, a utilização do gerenciador de versões de pacotes pode ser extremamente útil para uma equipe que trabalha com aplicações em microservices.
Tendo como base a equipe utilize uma esteira de CI/CD, pode ser possível automatizar a utilização desse arquivo durante o processo de `build` da aplicação.
Por exemplo:

### Configurações iniciais
1. Cria-se um repositório compartilhado para configurações de CI/CD

![image](https://github.com/user-attachments/assets/74cd36e6-5ed2-4f99-a216-8919f03797ed)

2. Cria-se, no repositório de cada MS, a esteira de pipeline que ficará responsável por copiar o arquivo `Directory.Packages.props` do repositório compartilhado para dentro do diretório do build da pipeline.

![image](https://github.com/user-attachments/assets/b5abb698-9193-48ba-bbb6-26546833e4e9)

```yaml
- task: CopyFiles@2 # Copiando o arquivo de configuração para a pasta atual do projeto em que se encontra a Solution
  inputs:
    SourceFolder: './nuget-centralized-dependencies-file' # Diretório no qual encontra-se o arquivo de configuração das versões dos pacotes
    contents: 'Directory.Packages.props'
    targetFolder: './nuget-centralized-dependencies-file-app01/' # Diretório onde se encontra o projeto atual
```
[Arquivo completo da pipeline](https://github.com/martineli17/.net-centralized-nuget-packages-versions/blob/master/azure-pipelines.yml)

3. Com isso, na execução do restore e build da aplicação via pipeline, o arquivo de configuração de versões de pacotes estará presente no diretório atual e será utilizado.

### Fluxo otimizado para o desenvolvedor 
1. O desenvolvedor mantém localmente o arquivo `Directory.Packages.props` dentro da solution, contendo as versões que deseja utilizar durante o processo de desenvolvimento. Esse arquivo é ignorado pelo `.git`.
2. O desenvolvedor faz alguma alteração no repositório que tem os pacotes comuns da equipe e gera uma nova versão de pré-release.
3. Ao gerar a nova versão de pré-release, o desenvolvedor altera as versões no arquivo `Directory.Packages.props` local dele e valida o funcionamento.
4. O desenvolvedor confirma que está tudo dentro do esperado, gera uma versão final de release dos pacotes comuns.
5. Com a versão release gerada, invés de alterar cada MS com a nova versão, ele altera somente o arquivo `Directory.Packages.props` que está presente no repositório de configurações compartilhadas.
