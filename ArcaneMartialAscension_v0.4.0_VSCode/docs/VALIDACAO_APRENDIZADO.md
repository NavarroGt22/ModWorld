# Primeiro protótipo de aprendizado autônomo — 27/10/2026

## Escopo implementado

O pedido de iniciar aprendizado autônomo antecipou um protótipo das Fases 4/6.
O marco principal do roadmap continua sendo a Fase 1, sem aceite final de
magia em combate. Este trabalho não torna a IA ou a progressão estáveis.

### Estudo

- Apenas habitantes vivos, sapientes e adultos são considerados.
- Magos existentes, afinidade de Evocação ou vocação estável por ID permitem
  estudo próprio. A seleção por ID usa um limite configurado de 12/100;
  não sorteia novamente a cada tick. Bruxos/feiticeiros exclusivos não recebem
  um caminho de Mago, pacto ou linhagem inventados pelo observador.
- Currículo inicial: Centelha Rubra, Estilhaço Glacial, Lança Elétrica e
  Rajada de Cinzas. Apenas círculos 0–1 e entradas com protótipo disponível.
- Sessão de estudo: 8 unidades de tempo do mundo, 5 XP; 32 de trabalho
  observado para círculo 0 e 64 para círculo 1. Requisitos de círculo permanecem.
- Sem arma ou outra interrupção, o primeiro feitiço exige pelo menos 32
  unidades observadas. Com prática simultânea, o tempo é dividido entre as
  atividades. Lotes grandes e observações espaçadas podem tornar o estudo mais lento.
- A origem registra método, ID do aprendiz e instante do mundo. A consulta
  da interface não cria conhecimentos. Ações de aprendizado do jogador
  continuam identificadas como intervenção do jogador.
- Inicializar estado não concede mais Centelha, XP de Mago, pacto de teste
  ou origem elemental. O repertório agora possui uma única fonte no modelo arcano.

### Armas

- A arma real vem de `Actor.getWeapon()` e `Item.getAsset()`, APIs verificadas
  na DLL local. O campo `equipment.weapon` representa um slot.
- Prática em paz: 2 XP por sessão de 10 unidades observadas, para a arma
  equipada. Trocar armas não transfere trabalho parcial ou XP.
- Combate: até 2 XP por frame de ataque válido do personagem; múltiplos
  callbacks de traços no mesmo frame não multiplicam a recompensa.
  Isso mede callbacks de ataque, não confirma acerto de cada flecha.
- Bônus/traços de armas e tiers fora do equipamento atual são removidos;
  XP permanece separado. Só o tier qualificado mais alto fica ativo.
- Prática e combate autônomos param no nível 7 (2.500 XP).
- O catálogo de técnicas permanece uma prévia sem executor de combate.

### Tempo e execução

- Lotes de até 24 atores a cada 0,5 s, reaproveitando a lista nativa de vivos.
- Tempo do mundo por `MapBox.getCurWorldTime()`. Jogo pausado não avança aprendizado.
- Combate e sono suspendem estudo/prática em paz; não são contados ao retornar.
- Intervalos longos são limitados a 10 unidades observadas por avaliação;
  não se concede anos de treino por uma única observação tardia.
- O tempo disponível é compartilhado quando estudo e prática coexistem.
- F6 controla a opção experimental. Desligar/religar não acumula XP do intervalo.
- Troca de mundo limpa estados em memória e projéteis antigos. Ainda não há
  serialização nem restauração de progressão por save.

## Verificações executadas

- `scripts/build.ps1`: compilação com DLLs reais, usando Roslyn NML 4.9.0.0.
- `scripts/test-learning.ps1`: 16 fontes compiladas, zero erros e avisos;
  **24 cenários passaram, zero falhas**.
- Os cenários cobrem estado inicial vazio, estudo gradual, origens, limites
  de círculo e nível, inelegibilidade, ausência de tempo, separação entre
  personagens/armas, troca de equipamento, deduplicação de combate,
  tempo inválido, grandes saltos, desativação, divisão de tempo, requisitos
  manuais, identificação de armas, vocação estável e interrupção por combate.

Esses testes executam as regras C# em um processo de teste; não executam o
WorldBox nem substituem a validação dos callbacks ou do observador no jogo.

O protótipo foi instalado com o WorldBox fechado. Backups desta instalação:

- Saves/autosaves: `C:\Users\rn558\Documents\AMA_TestBackups\Learning_20261027_223754`.
- Mod anterior: `C:\Program Files (x86)\Steam\steamapps\common\worldbox\AMA_Backups\ArcaneMartialAscension_20261027_223756`.

Catálogo, JSONs e sintaxe dos scripts foram validados. Nenhuma DLL do jogo
foi incluída no mod instalado. O jogo não foi controlado ou reiniciado nesta etapa.

## Validação no jogo ainda pendente

1. Fazer backup e abrir um mundo descartável com adultos e equipamentos.
2. Conferir o toggle de aprendizado em F6 e os estados nas abas.
3. Observar um Mago sem magia aprender Centelha por estudo próprio.
4. Observar aumento de XP em cada arma; trocar e retirar armas.
5. Conferir uma recompensa por ataque mesmo com traços legados múltiplos.
6. Pausar, desligar/religar e entrar em combate sem conceder treino retroativo.
7. Verificar logs `[AMA Learning]` e ausência de erros repetidos.
8. Trocar de mundo e verificar ausência de dados do mapa anterior.

## Próximos passos do roadmap

- Validar primeiro uma conjuração completa e a maestria no WorldBox (Fases 1/2).
- Implementar persistência versionada antes de declarar a evolução estável (Fase 5).
- Expandir fontes de conhecimento, instituições e decisões sociais (Fases 4/6/7).
- Implementar efeitos próprios e testes de conteúdo além desse currículo inicial.
