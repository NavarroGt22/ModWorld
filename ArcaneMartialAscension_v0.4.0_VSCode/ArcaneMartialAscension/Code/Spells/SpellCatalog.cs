using System;
using System.Collections.Generic;
using ArcaneMartialAscension.Core;

namespace ArcaneMartialAscension.Spells
{
    public sealed class SpellEntry
    {
        public readonly string Id, Name, Category, Target, Description;
        public readonly MagicSchool School;
        public readonly int Circle;
        public readonly float Mana, Cooldown;
        public SpellEntry(string id, string name, MagicSchool school, int circle,
            float mana, float cooldown, string category, string target, string description)
        {
            Id=id; Name=name; School=school; Circle=circle; Mana=mana;
            Cooldown=cooldown; Category=category; Target=target; Description=description;
        }
    }
    // Catalog stored directly in compiled C# source; no separate install or asset required.
    public static class SpellCatalog
    {
        public static readonly List<SpellEntry> All = new List<SpellEntry>(160);
        private static readonly Dictionary<string, SpellEntry> ById =
            new Dictionary<string, SpellEntry>(StringComparer.Ordinal);
        static SpellCatalog()
        {
            Add(new SpellEntry("ama.spell.abjuration.0.centelha_guardia", "Centelha Guardiã", MagicSchool.Abjuration, 0, 3f, 2f, "escudo", "ally", "Concede um pequeno escudo temporário a uma unidade."));
            Add(new SpellEntry("ama.spell.abjuration.0.repulsao_runica", "Repulsão Rúnica", MagicSchool.Abjuration, 0, 3f, 2f, "controle", "enemy", "Empurra levemente o atacante mais próximo, sem causar dano."));
            Add(new SpellEntry("ama.spell.abjuration.1.escudo_prismatico", "Escudo Prismático", MagicSchool.Abjuration, 1, 10f, 4f, "escudo", "ally", "Absorve dano recebido pelo alvo até romper ou expirar."));
            Add(new SpellEntry("ama.spell.abjuration.1.passo_da_muralha", "Passo da Muralha", MagicSchool.Abjuration, 1, 10f, 4f, "melhoria", "ally", "Reduz o próximo golpe recebido e concede resistência breve."));
            Add(new SpellEntry("ama.spell.abjuration.2.circulo_antifeitico", "Círculo Antifeitiço", MagicSchool.Abjuration, 2, 18f, 7f, "dissipacao", "ally", "Remove um encantamento negativo de baixa intensidade."));
            Add(new SpellEntry("ama.spell.abjuration.2.bastiao_de_pedra", "Bastião de Pedra", MagicSchool.Abjuration, 2, 18f, 7f, "barreira", "point", "Cria barreira temporária que impede passagem de projéteis."));
            Add(new SpellEntry("ama.spell.abjuration.3.egide_da_companhia", "Égide da Companhia", MagicSchool.Abjuration, 3, 30f, 11f, "escudo", "area", "Distribui proteção entre aliados próximos, até seis unidades."));
            Add(new SpellEntry("ama.spell.abjuration.3.quebra_de_maldicao", "Quebra de Maldição", MagicSchool.Abjuration, 3, 30f, 11f, "dissipacao", "area", "Remove efeitos de medo, veneno mágico ou enfraquecimento de aliados."));
            Add(new SpellEntry("ama.spell.abjuration.4.cupula_das_sete_runas", "Cúpula das Sete Runas", MagicSchool.Abjuration, 4, 45f, 17f, "barreira", "area", "Cria domo defensivo sobre um grupo pequeno."));
            Add(new SpellEntry("ama.spell.abjuration.4.espelho_arcano", "Espelho Arcano", MagicSchool.Abjuration, 4, 45f, 17f, "reflexao", "ally", "Reflete um projétil mágico, limitado a magia de círculo igual ou inferior."));
            Add(new SpellEntry("ama.spell.abjuration.5.ancora_planar", "Âncora Planar", MagicSchool.Abjuration, 5, 65f, 25f, "barreira", "area", "Impede teleporte e invocações hostis dentro da área."));
            Add(new SpellEntry("ama.spell.abjuration.5.muralha_de_silencio", "Muralha de Silêncio", MagicSchool.Abjuration, 5, 65f, 25f, "controle", "area", "Interrompe conjurações dentro da faixa protegida."));
            Add(new SpellEntry("ama.spell.abjuration.6.fortaleza_viva", "Fortaleza Viva", MagicSchool.Abjuration, 6, 90f, 36f, "escudo", "area", "Mantém escudos regenerativos em até oito aliados."));
            Add(new SpellEntry("ama.spell.abjuration.6.manto_inquebravel", "Manto Inquebrável", MagicSchool.Abjuration, 6, 90f, 36f, "melhoria", "ally", "Reduz em até sessenta por cento o dano por duração curta."));
            Add(new SpellEntry("ama.spell.abjuration.7.dominio_do_lacre", "Domínio do Lacre", MagicSchool.Abjuration, 7, 120f, 50f, "controle", "area", "Bloqueia novas magias inimigas de círculo baixo dentro da zona."));
            Add(new SpellEntry("ama.spell.abjuration.7.ricochete_soberano", "Ricochete Soberano", MagicSchool.Abjuration, 7, 120f, 50f, "reflexao", "area", "Reflete uma fração do dano mágico recebido, com teto por conjuração."));
            Add(new SpellEntry("ama.spell.abjuration.8.prisao_do_vazio", "Prisão do Vazio", MagicSchool.Abjuration, 8, 160f, 75f, "controle", "enemy", "Sela temporariamente a conjuração de um inimigo poderoso."));
            Add(new SpellEntry("ama.spell.abjuration.8.refugio_absoluto", "Refúgio Absoluto", MagicSchool.Abjuration, 8, 160f, 75f, "escudo", "area", "Impede dano em até quatro aliados por poucos segundos; não protege de terreno letal."));
            Add(new SpellEntry("ama.spell.abjuration.9.lei_da_impenetrabilidade", "Lei da Impenetrabilidade", MagicSchool.Abjuration, 9, 210f, 120f, "barreira", "area", "Ergue uma fortaleza arcana de área limitada, resistente a investidas."));
            Add(new SpellEntry("ama.spell.abjuration.9.banimento_da_catastrofe", "Banimento da Catástrofe", MagicSchool.Abjuration, 9, 210f, 120f, "dissipacao", "area", "Cancela um feitiço de área já iniciado, se não exceder o nível do conjurador."));
            Add(new SpellEntry("ama.spell.conjuration.0.agulha_eterea", "Agulha Etérea", MagicSchool.Conjuration, 0, 3f, 2f, "dano", "enemy", "Invoca uma agulha espectral que perfura um inimigo."));
            Add(new SpellEntry("ama.spell.conjuration.0.mao_passageira", "Mão Passageira", MagicSchool.Conjuration, 0, 3f, 2f, "controle", "point", "Cria uma mão de força que empurra um inimigo ou objeto próximo."));
            Add(new SpellEntry("ama.spell.conjuration.1.lamina_invocada", "Lâmina Invocada", MagicSchool.Conjuration, 1, 10f, 4f, "dano", "enemy", "Cria arma espectral para um ataque à distância curta."));
            Add(new SpellEntry("ama.spell.conjuration.1.sentinela_de_poeira", "Sentinela de Poeira", MagicSchool.Conjuration, 1, 10f, 4f, "invocacao", "point", "Invoca uma criatura frágil que distrai um inimigo."));
            Add(new SpellEntry("ama.spell.conjuration.2.cao_de_cinzas", "Cão de Cinzas", MagicSchool.Conjuration, 2, 18f, 7f, "invocacao", "point", "Invoca um familiar veloz para atacar e farejar inimigos."));
            Add(new SpellEntry("ama.spell.conjuration.2.seta_de_passagem", "Seta de Passagem", MagicSchool.Conjuration, 2, 18f, 7f, "teleporte", "ally", "Teleporta uma unidade aliada por curta distância para posição livre."));
            Add(new SpellEntry("ama.spell.conjuration.3.portal_de_assalto", "Portal de Assalto", MagicSchool.Conjuration, 3, 30f, 11f, "teleporte", "area", "Desloca até três aliados para outra posição próxima e segura."));
            Add(new SpellEntry("ama.spell.conjuration.3.guardiao_de_bronze", "Guardião de Bronze", MagicSchool.Conjuration, 3, 30f, 11f, "invocacao", "point", "Invoca um defensor temporário com alto bloqueio."));
            Add(new SpellEntry("ama.spell.conjuration.4.chuva_de_lancas", "Chuva de Lanças", MagicSchool.Conjuration, 4, 45f, 17f, "dano_em_area", "area", "Materializa lanças sobre uma zona, causando dano múltiplo."));
            Add(new SpellEntry("ama.spell.conjuration.4.passagem_geminada", "Passagem Geminada", MagicSchool.Conjuration, 4, 45f, 17f, "teleporte", "area", "Teleporta até cinco aliados entre dois pontos visíveis."));
            Add(new SpellEntry("ama.spell.conjuration.5.cavaleiro_astral", "Cavaleiro Astral", MagicSchool.Conjuration, 5, 65f, 25f, "invocacao", "point", "Invoca uma unidade montada espectral com duração limitada."));
            Add(new SpellEntry("ama.spell.conjuration.5.selo_de_reforco", "Selo de Reforço", MagicSchool.Conjuration, 5, 65f, 25f, "invocacao", "point", "Invoca três guerreiros etéreos para proteção local."));
            Add(new SpellEntry("ama.spell.conjuration.6.legiao_de_vidro", "Legião de Vidro", MagicSchool.Conjuration, 6, 90f, 36f, "invocacao", "point", "Invoca até seis defensores frágeis por tempo limitado."));
            Add(new SpellEntry("ama.spell.conjuration.6.ponte_do_vazio", "Ponte do Vazio", MagicSchool.Conjuration, 6, 90f, 36f, "teleporte", "area", "Abre passagem curta sobre água ou obstáculo para oito aliados."));
            Add(new SpellEntry("ama.spell.conjuration.7.colosso_de_obsidiana", "Colosso de Obsidiana", MagicSchool.Conjuration, 7, 120f, 50f, "invocacao", "point", "Invoca um guardião lento, grande e resistente."));
            Add(new SpellEntry("ama.spell.conjuration.7.portal_de_cerco", "Portal de Cerco", MagicSchool.Conjuration, 7, 120f, 50f, "teleporte", "area", "Reposiciona até dez aliados num ponto sem ocupação inimiga."));
            Add(new SpellEntry("ama.spell.conjuration.8.mare_de_invocados", "Maré de Invocados", MagicSchool.Conjuration, 8, 160f, 75f, "invocacao", "area", "Materializa até doze soldados efêmeros, sujeitos ao limite global de invocações."));
            Add(new SpellEntry("ama.spell.conjuration.8.teia_de_portais", "Teia de Portais", MagicSchool.Conjuration, 8, 160f, 75f, "teleporte", "area", "Encadeia três saltos curtos de um grupo aliado."));
            Add(new SpellEntry("ama.spell.conjuration.9.hoste_do_firmamento", "Hoste do Firmamento", MagicSchool.Conjuration, 9, 210f, 120f, "invocacao", "area", "Invoca até dezesseis guardiões celestes temporários."));
            Add(new SpellEntry("ama.spell.conjuration.9.travessia_continental", "Travessia Continental", MagicSchool.Conjuration, 9, 210f, 120f, "teleporte", "area", "Reposiciona até vinte aliados entre dois pontos aliados previamente marcados."));
            Add(new SpellEntry("ama.spell.divination.0.lampejo_de_intencoes", "Lampejo de Intenções", MagicSchool.Divination, 0, 3f, 2f, "deteccao", "self", "Revela a direção aproximada de inimigos próximos."));
            Add(new SpellEntry("ama.spell.divination.0.olho_de_poeira", "Olho de Poeira", MagicSchool.Divination, 0, 3f, 2f, "deteccao", "enemy", "Destaca temporariamente uma unidade escondida no alcance."));
            Add(new SpellEntry("ama.spell.divination.1.sentido_de_emboscada", "Sentido de Emboscada", MagicSchool.Divination, 1, 10f, 4f, "deteccao", "area", "Exibe inimigos ocultos próximos dos aliados."));
            Add(new SpellEntry("ama.spell.divination.1.mapa_de_mana", "Mapa de Mana", MagicSchool.Divination, 1, 10f, 4f, "deteccao", "enemy", "Mostra o potencial mágico de uma unidade examinada."));
            Add(new SpellEntry("ama.spell.divination.2.marca_de_pressagio", "Marca de Presságio", MagicSchool.Divination, 2, 18f, 7f, "melhoria", "ally", "Aumenta a precisão do próximo ataque de um aliado."));
            Add(new SpellEntry("ama.spell.divination.2.desvelar_miragens", "Desvelar Miragens", MagicSchool.Divination, 2, 18f, 7f, "dissipacao", "area", "Remove ilusões básicas dentro de uma área pequena."));
            Add(new SpellEntry("ama.spell.divination.3.visao_da_linha_de_tiro", "Visão da Linha de Tiro", MagicSchool.Divination, 3, 30f, 11f, "melhoria", "area", "Melhora temporariamente a precisão dos arqueiros aliados."));
            Add(new SpellEntry("ama.spell.divination.3.prever_golpe", "Prever Golpe", MagicSchool.Divination, 3, 30f, 11f, "melhoria", "ally", "Permite evitar uma única investida iminente."));
            Add(new SpellEntry("ama.spell.divination.4.cartografia_viva", "Cartografia Viva", MagicSchool.Divination, 4, 45f, 17f, "deteccao", "area", "Revela movimentos inimigos próximos por curto período."));
            Add(new SpellEntry("ama.spell.divination.4.leitura_de_fraqueza", "Leitura de Fraqueza", MagicSchool.Divination, 4, 45f, 17f, "enfraquecimento", "enemy", "Marca um inimigo que passa a receber mais dano físico."));
            Add(new SpellEntry("ama.spell.divination.5.oraculo_de_batalha", "Oráculo de Batalha", MagicSchool.Divination, 5, 65f, 25f, "melhoria", "area", "Melhora esquivas e contra-ataques de um pelotão aliado."));
            Add(new SpellEntry("ama.spell.divination.5.espelho_distante", "Espelho Distante", MagicSchool.Divination, 5, 65f, 25f, "deteccao", "point", "Explora por alguns segundos um ponto remoto do mapa."));
            Add(new SpellEntry("ama.spell.divination.6.nos_do_destino", "Nós do Destino", MagicSchool.Divination, 6, 90f, 36f, "melhoria", "area", "Protege um pequeno grupo contra um acerto crítico."));
            Add(new SpellEntry("ama.spell.divination.6.profecia_da_ruptura", "Profecia da Ruptura", MagicSchool.Divination, 6, 90f, 36f, "deteccao", "area", "Destaca inimigos preparando feitiços de alto círculo."));
            Add(new SpellEntry("ama.spell.divination.7.ruptura_da_probabilidade", "Ruptura da Probabilidade", MagicSchool.Divination, 7, 120f, 50f, "enfraquecimento", "area", "Faz ataques hostis na área errarem com mais frequência."));
            Add(new SpellEntry("ama.spell.divination.7.convergencia_tatica", "Convergência Tática", MagicSchool.Divination, 7, 120f, 50f, "melhoria", "area", "Melhora o foco de alvo e a movimentação de aliados."));
            Add(new SpellEntry("ama.spell.divination.8.mente_de_mil_olhos", "Mente de Mil Olhos", MagicSchool.Divination, 8, 160f, 75f, "deteccao", "area", "Revela unidades e armadilhas em uma ampla região."));
            Add(new SpellEntry("ama.spell.divination.8.pressagio_de_extincao", "Presságio de Extinção", MagicSchool.Divination, 8, 160f, 75f, "melhoria", "area", "Destaca inimigos vulneráveis e concede evasão a aliados."));
            Add(new SpellEntry("ama.spell.divination.9.decreto_do_inevitavel", "Decreto do Inevitável", MagicSchool.Divination, 9, 210f, 120f, "melhoria", "area", "Concede previsão de um golpe a até dez aliados; não garante vitória."));
            Add(new SpellEntry("ama.spell.divination.9.mapa_dos_futuros", "Mapa dos Futuros", MagicSchool.Divination, 9, 210f, 120f, "deteccao", "area", "Mostra zonas prováveis de impacto de grandes feitiços por tempo curto."));
            Add(new SpellEntry("ama.spell.enchantment.0.murmurio_de_calma", "Murmúrio de Calma", MagicSchool.Enchantment, 0, 3f, 2f, "controle", "enemy", "Retarda a próxima ação ofensiva de um inimigo."));
            Add(new SpellEntry("ama.spell.enchantment.0.fio_de_simpatia", "Fio de Simpatia", MagicSchool.Enchantment, 0, 3f, 2f, "enfraquecimento", "enemy", "Reduz a chance de um inimigo escolher o conjurador como alvo."));
            Add(new SpellEntry("ama.spell.enchantment.1.ordem_breve", "Ordem Breve", MagicSchool.Enchantment, 1, 10f, 4f, "controle", "enemy", "Atordoa um inimigo por menos de um segundo, sujeito a resistência."));
            Add(new SpellEntry("ama.spell.enchantment.1.coragem_encantada", "Coragem Encantada", MagicSchool.Enchantment, 1, 10f, 4f, "melhoria", "ally", "Aumenta temporariamente a resistência ao medo de um aliado."));
            Add(new SpellEntry("ama.spell.enchantment.2.medo_sussurrante", "Medo Sussurrante", MagicSchool.Enchantment, 2, 18f, 7f, "controle", "enemy", "Faz um inimigo recuar durante alguns instantes."));
            Add(new SpellEntry("ama.spell.enchantment.2.laco_de_obediencia", "Laço de Obediência", MagicSchool.Enchantment, 2, 18f, 7f, "controle", "enemy", "Faz um inimigo comum lutar ao lado do conjurador por tempo curto."));
            Add(new SpellEntry("ama.spell.enchantment.3.canto_da_harmonia", "Canto da Harmonia", MagicSchool.Enchantment, 3, 30f, 11f, "melhoria", "area", "Aumenta a recuperação de mana de aliados próximos."));
            Add(new SpellEntry("ama.spell.enchantment.3.quebra_de_vontade", "Quebra de Vontade", MagicSchool.Enchantment, 3, 30f, 11f, "enfraquecimento", "enemy", "Reduz velocidade e resistência mental de um inimigo."));
            Add(new SpellEntry("ama.spell.enchantment.4.aura_de_tregua", "Aura de Trégua", MagicSchool.Enchantment, 4, 45f, 17f, "controle", "area", "Impede ataques de unidades comuns numa região por instantes."));
            Add(new SpellEntry("ama.spell.enchantment.4.fervor_conquistador", "Fervor Conquistador", MagicSchool.Enchantment, 4, 45f, 17f, "melhoria", "area", "Aumenta a velocidade de ataque de aliados próximos."));
            Add(new SpellEntry("ama.spell.enchantment.5.dominio_dos_sentidos", "Domínio dos Sentidos", MagicSchool.Enchantment, 5, 65f, 25f, "enfraquecimento", "area", "Diminui bastante a precisão de inimigos na área."));
            Add(new SpellEntry("ama.spell.enchantment.5.pacto_de_lealdade", "Pacto de Lealdade", MagicSchool.Enchantment, 5, 65f, 25f, "melhoria", "area", "Impede fuga por medo de aliados por curto período."));
            Add(new SpellEntry("ama.spell.enchantment.6.palavra_de_silencio", "Palavra de Silêncio", MagicSchool.Enchantment, 6, 90f, 36f, "controle", "enemy", "Bloqueia a conjuração de um inimigo por alguns segundos."));
            Add(new SpellEntry("ama.spell.enchantment.6.extase_marcial", "Êxtase Marcial", MagicSchool.Enchantment, 6, 90f, 36f, "melhoria", "area", "Concede velocidade de ataque e sinergia com maestria de espada."));
            Add(new SpellEntry("ama.spell.enchantment.7.comando_coral", "Comando Coral", MagicSchool.Enchantment, 7, 120f, 50f, "controle", "area", "Imobiliza por instantes até oito inimigos comuns."));
            Add(new SpellEntry("ama.spell.enchantment.7.coroa_da_determinacao", "Coroa da Determinação", MagicSchool.Enchantment, 7, 120f, 50f, "melhoria", "area", "Protege aliados contra medo e controle mental."));
            Add(new SpellEntry("ama.spell.enchantment.8.decreto_do_esquecimento", "Decreto do Esquecimento", MagicSchool.Enchantment, 8, 160f, 75f, "dissipacao", "enemy", "Remove vários reforços mágicos temporários de um inimigo."));
            Add(new SpellEntry("ama.spell.enchantment.8.marcha_hipnotica", "Marcha Hipnótica", MagicSchool.Enchantment, 8, 160f, 75f, "controle", "area", "Obriga até seis inimigos comuns a avançar rumo a ponto marcado."));
            Add(new SpellEntry("ama.spell.enchantment.9.vontade_do_soberano", "Vontade do Soberano", MagicSchool.Enchantment, 9, 210f, 120f, "controle", "area", "Controla temporariamente até quatro inimigos comuns; não controla chefes."));
            Add(new SpellEntry("ama.spell.enchantment.9.juramento_de_paz", "Juramento de Paz", MagicSchool.Enchantment, 9, 210f, 120f, "controle", "area", "Suspende ataques em uma zona por até oito segundos."));
            Add(new SpellEntry("ama.spell.evocation.0.centelha_rubra", "Centelha Rubra", MagicSchool.Evocation, 0, 3f, 2f, "dano", "enemy", "Dispara pequena fagulha de fogo contra uma unidade."));
            Add(new SpellEntry("ama.spell.evocation.0.estilhaco_glacial", "Estilhaço Glacial", MagicSchool.Evocation, 0, 3f, 2f, "dano", "enemy", "Lança fragmento de gelo que causa dano e lentidão curta."));
            Add(new SpellEntry("ama.spell.evocation.1.lanca_eletrica", "Lança Elétrica", MagicSchool.Evocation, 1, 10f, 4f, "dano", "enemy", "Dispara raio elétrico instantâneo sobre um alvo."));
            Add(new SpellEntry("ama.spell.evocation.1.rajada_de_cinzas", "Rajada de Cinzas", MagicSchool.Evocation, 1, 10f, 4f, "dano_em_area", "area", "Projeta cone curto de chamas e cinzas."));
            Add(new SpellEntry("ama.spell.evocation.2.onda_de_fogo", "Onda de Fogo", MagicSchool.Evocation, 2, 18f, 7f, "dano_em_area", "area", "Incendeia uma área pequena por um instante."));
            Add(new SpellEntry("ama.spell.evocation.2.agulhas_de_gelo", "Agulhas de Gelo", MagicSchool.Evocation, 2, 18f, 7f, "dano", "enemy", "Lança quatro fragmentos contra alvos próximos."));
            Add(new SpellEntry("ama.spell.evocation.3.meteoro_menor", "Meteoro Menor", MagicSchool.Evocation, 3, 30f, 11f, "dano_em_area", "point", "Invoca impacto ardente com pequena explosão de área."));
            Add(new SpellEntry("ama.spell.evocation.3.corrente_de_trovao", "Corrente de Trovão", MagicSchool.Evocation, 3, 30f, 11f, "dano", "enemy", "Raio atinge até três inimigos, perdendo força a cada salto."));
            Add(new SpellEntry("ama.spell.evocation.4.coluna_de_fogo", "Coluna de Fogo", MagicSchool.Evocation, 4, 45f, 17f, "dano_em_area", "point", "Cria coluna flamejante numa zona marcada."));
            Add(new SpellEntry("ama.spell.evocation.4.cratera_de_granizo", "Cratera de Granizo", MagicSchool.Evocation, 4, 45f, 17f, "dano_em_area", "area", "Causa dano de gelo e reduz a velocidade na zona."));
            Add(new SpellEntry("ama.spell.evocation.5.serpente_de_relampago", "Serpente de Relâmpago", MagicSchool.Evocation, 5, 65f, 25f, "dano", "enemy", "Relâmpago encadeado pode atingir até cinco alvos."));
            Add(new SpellEntry("ama.spell.evocation.5.mare_vulcanica", "Maré Vulcânica", MagicSchool.Evocation, 5, 65f, 25f, "dano_em_area", "area", "Onda larga de lava mágica causa dano e empurra."));
            Add(new SpellEntry("ama.spell.evocation.6.tempestade_ignea", "Tempestade Ígnea", MagicSchool.Evocation, 6, 90f, 36f, "dano_em_area", "area", "Mantém chuva de chamas em região limitada."));
            Add(new SpellEntry("ama.spell.evocation.6.arco_solar", "Arco Solar", MagicSchool.Evocation, 6, 90f, 36f, "dano_em_area", "area", "Dispara feixe de luz em linha reta atravessando inimigos."));
            Add(new SpellEntry("ama.spell.evocation.7.aurora_devastadora", "Aurora Devastadora", MagicSchool.Evocation, 7, 120f, 50f, "dano_em_area", "area", "Descarrega uma grande faixa de energia radiante."));
            Add(new SpellEntry("ama.spell.evocation.7.chuva_de_estrelas", "Chuva de Estrelas", MagicSchool.Evocation, 7, 120f, 50f, "dano_em_area", "area", "Convoca impactos sequenciais sobre uma zona."));
            Add(new SpellEntry("ama.spell.evocation.8.coracao_da_tempestade", "Coração da Tempestade", MagicSchool.Evocation, 8, 160f, 75f, "dano_em_area", "area", "Cria zona elétrica persistente com raios periódicos."));
            Add(new SpellEntry("ama.spell.evocation.8.sol_despedacado", "Sol Despedaçado", MagicSchool.Evocation, 8, 160f, 75f, "dano_em_area", "point", "Causa explosão solar forte seguida de queima breve."));
            Add(new SpellEntry("ama.spell.evocation.9.cataclismo_elemental", "Cataclismo Elemental", MagicSchool.Evocation, 9, 210f, 120f, "dano_em_area", "area", "Três ondas consecutivas de fogo, gelo e raios sobre região ampla."));
            Add(new SpellEntry("ama.spell.evocation.9.estrela_do_julgamento", "Estrela do Julgamento", MagicSchool.Evocation, 9, 210f, 120f, "dano_em_area", "point", "Invoca um impacto colossal em três estágios com aviso visual."));
            Add(new SpellEntry("ama.spell.illusion.0.sombra_duplicada", "Sombra Duplicada", MagicSchool.Illusion, 0, 3f, 2f, "ilusao", "point", "Cria imagem estacionária para desviar um ataque inimigo."));
            Add(new SpellEntry("ama.spell.illusion.0.veu_de_cor", "Véu de Cor", MagicSchool.Illusion, 0, 3f, 2f, "ilusao", "ally", "Camufla uma unidade brevemente sem impedir dano recebido."));
            Add(new SpellEntry("ama.spell.illusion.1.clone_de_fumaca", "Clone de Fumaça", MagicSchool.Illusion, 1, 10f, 4f, "ilusao", "point", "Invoca cópia falsa que desaparece após receber um golpe."));
            Add(new SpellEntry("ama.spell.illusion.1.passo_invisivel", "Passo Invisível", MagicSchool.Illusion, 1, 10f, 4f, "ilusao", "ally", "Torna uma unidade difícil de perceber até atacar."));
            Add(new SpellEntry("ama.spell.illusion.2.exercito_eco", "Exército-Eco", MagicSchool.Illusion, 2, 18f, 7f, "ilusao", "area", "Cria três soldados ilusórios sem dano real."));
            Add(new SpellEntry("ama.spell.illusion.2.desvio_de_perspectiva", "Desvio de Perspectiva", MagicSchool.Illusion, 2, 18f, 7f, "enfraquecimento", "enemy", "Reduz precisão dos ataques dirigidos contra um aliado."));
            Add(new SpellEntry("ama.spell.illusion.3.labirinto_de_miragens", "Labirinto de Miragens", MagicSchool.Illusion, 3, 30f, 11f, "ilusao", "area", "Confunde a seleção de alvos de inimigos numa área pequena."));
            Add(new SpellEntry("ama.spell.illusion.3.silencio_do_espelho", "Silêncio do Espelho", MagicSchool.Illusion, 3, 30f, 11f, "ilusao", "ally", "Oculta sinais visuais e sonoros de movimento por instantes."));
            Add(new SpellEntry("ama.spell.illusion.4.falsos_reforcos", "Falsos Reforços", MagicSchool.Illusion, 4, 45f, 17f, "ilusao", "area", "Projeta um grupo de soldados falsos que provoca recuo tático."));
            Add(new SpellEntry("ama.spell.illusion.4.duplicata_avancada", "Duplicata Avançada", MagicSchool.Illusion, 4, 45f, 17f, "ilusao", "point", "Cria cópia móvel que simula ataques sem causar dano."));
            Add(new SpellEntry("ama.spell.illusion.5.eclipse_dos_olhos", "Eclipse dos Olhos", MagicSchool.Illusion, 5, 65f, 25f, "enfraquecimento", "area", "Reduz severamente visão e precisão dos inimigos na zona."));
            Add(new SpellEntry("ama.spell.illusion.5.troca_de_imagens", "Troca de Imagens", MagicSchool.Illusion, 5, 65f, 25f, "teleporte", "ally", "Troca a posição de uma unidade com uma imagem próxima."));
            Add(new SpellEntry("ama.spell.illusion.6.salao_de_espelhos", "Salão de Espelhos", MagicSchool.Illusion, 6, 90f, 36f, "ilusao", "area", "Cria até oito cópias móveis distribuídas ao redor."));
            Add(new SpellEntry("ama.spell.illusion.6.ocultar_batalhao", "Ocultar Batalhão", MagicSchool.Illusion, 6, 90f, 36f, "ilusao", "area", "Camufla até oito aliados até o primeiro ataque."));
            Add(new SpellEntry("ama.spell.illusion.7.cidade_fantasma", "Cidade Fantasma", MagicSchool.Illusion, 7, 120f, 50f, "ilusao", "area", "Projeta estruturas ilusórias temporárias que confundem invasores."));
            Add(new SpellEntry("ama.spell.illusion.7.duplo_arcanista", "Duplo Arcanista", MagicSchool.Illusion, 7, 120f, 50f, "ilusao", "point", "Cria cópia que simula uma conjuração sem causar dano real."));
            Add(new SpellEntry("ama.spell.illusion.8.mascara_da_realidade", "Máscara da Realidade", MagicSchool.Illusion, 8, 160f, 75f, "ilusao", "area", "Mostra perigos fictícios e desorienta unidades comuns."));
            Add(new SpellEntry("ama.spell.illusion.8.desaparecimento_coletivo", "Desaparecimento Coletivo", MagicSchool.Illusion, 8, 160f, 75f, "ilusao", "area", "Oculta até quinze aliados por curta duração."));
            Add(new SpellEntry("ama.spell.illusion.9.teatro_dos_mil_veus", "Teatro dos Mil Véus", MagicSchool.Illusion, 9, 210f, 120f, "ilusao", "area", "Cria até trinta combatentes falsos para distrair um exército."));
            Add(new SpellEntry("ama.spell.illusion.9.veu_do_mundo", "Véu do Mundo", MagicSchool.Illusion, 9, 210f, 120f, "ilusao", "area", "Camufla até vinte aliados e cria alvos falsos próximos."));
            Add(new SpellEntry("ama.spell.necromancy.0.toque_frio", "Toque Frio", MagicSchool.Necromancy, 0, 3f, 2f, "dano", "enemy", "Causa pequeno dano sombrio com lentidão momentânea."));
            Add(new SpellEntry("ama.spell.necromancy.0.eco_sepulcral", "Eco Sepulcral", MagicSchool.Necromancy, 0, 3f, 2f, "deteccao", "area", "Marca cadáveres recentes e revela unidades feridas nas proximidades."));
            Add(new SpellEntry("ama.spell.necromancy.1.dreno_de_vigor", "Dreno de Vigor", MagicSchool.Necromancy, 1, 10f, 4f, "dreno_vital", "enemy", "Rouba pouca vida de um inimigo e cura parcialmente o conjurador."));
            Add(new SpellEntry("ama.spell.necromancy.1.servo_osseo", "Servo Ósseo", MagicSchool.Necromancy, 1, 10f, 4f, "invocacao", "corpse", "Ergue um esqueleto fraco a partir de um cadáver recente."));
            Add(new SpellEntry("ama.spell.necromancy.2.nevoa_funeraria", "Névoa Funerária", MagicSchool.Necromancy, 2, 18f, 7f, "dano_periodico", "area", "Cria nuvem que causa dano sombrio por segundo."));
            Add(new SpellEntry("ama.spell.necromancy.2.mao_do_tumulo", "Mão do Túmulo", MagicSchool.Necromancy, 2, 18f, 7f, "controle", "enemy", "Enraíza um alvo comum por alguns segundos."));
            Add(new SpellEntry("ama.spell.necromancy.3.vinculo_sanguineo", "Vínculo Sanguíneo", MagicSchool.Necromancy, 3, 30f, 11f, "dreno_vital", "enemy", "Redireciona fração limitada do dano recebido para inimigo vinculado."));
            Add(new SpellEntry("ama.spell.necromancy.3.esquadrao_reanimado", "Esquadrão Reanimado", MagicSchool.Necromancy, 3, 30f, 11f, "invocacao", "corpse", "Reanima até três cadáveres em soldados fracos."));
            Add(new SpellEntry("ama.spell.necromancy.4.praga_cinzenta", "Praga Cinzenta", MagicSchool.Necromancy, 4, 45f, 17f, "dano_periodico", "enemy", "Infecção sombria se espalha para até dois inimigos próximos."));
            Add(new SpellEntry("ama.spell.necromancy.4.sombra_faminta", "Sombra Faminta", MagicSchool.Necromancy, 4, 45f, 17f, "invocacao", "point", "Invoca sombra que drena pequena quantidade de vida em combate."));
            Add(new SpellEntry("ama.spell.necromancy.5.coleta_de_almas", "Coleta de Almas", MagicSchool.Necromancy, 5, 65f, 25f, "melhoria", "self", "Recupera mana limitada quando criaturas marcadas morrem."));
            Add(new SpellEntry("ama.spell.necromancy.5.marcha_cadaverica", "Marcha Cadavérica", MagicSchool.Necromancy, 5, 65f, 25f, "invocacao", "corpse", "Reanima até cinco cadáveres por duração curta."));
            Add(new SpellEntry("ama.spell.necromancy.6.exercito_da_ultima_noite", "Exército da Última Noite", MagicSchool.Necromancy, 6, 90f, 36f, "invocacao", "corpse", "Ergue até oito mortos-vivos temporários de uma zona de batalha."));
            Add(new SpellEntry("ama.spell.necromancy.6.selo_de_decomposicao", "Selo de Decomposição", MagicSchool.Necromancy, 6, 90f, 36f, "enfraquecimento", "area", "Reduz armadura e resistência dos inimigos na área."));
            Add(new SpellEntry("ama.spell.necromancy.7.senhor_das_ossadas", "Senhor das Ossadas", MagicSchool.Necromancy, 7, 120f, 50f, "invocacao", "point", "Invoca um comandante morto-vivo único e resistente."));
            Add(new SpellEntry("ama.spell.necromancy.7.mortandade_circular", "Mortandade Circular", MagicSchool.Necromancy, 7, 120f, 50f, "dano_em_area", "area", "Provoca forte dano sombrio em área com limite de alvos."));
            Add(new SpellEntry("ama.spell.necromancy.8.eclipse_vital", "Eclipse Vital", MagicSchool.Necromancy, 8, 160f, 75f, "dreno_vital", "area", "Drena vida de inimigos numa área e cura parcialmente o conjurador."));
            Add(new SpellEntry("ama.spell.necromancy.8.arauto_funebre", "Arauto Fúnebre", MagicSchool.Necromancy, 8, 160f, 75f, "invocacao", "point", "Invoca um campeão espectral com aura de medo limitada."));
            Add(new SpellEntry("ama.spell.necromancy.9.trono_da_morte", "Trono da Morte", MagicSchool.Necromancy, 9, 210f, 120f, "invocacao", "corpse", "Reanima até dez cadáveres e concede aura de fortalecimento limitada."));
            Add(new SpellEntry("ama.spell.necromancy.9.grande_quietude", "Grande Quietude", MagicSchool.Necromancy, 9, 210f, 120f, "dano_periodico", "area", "Cria zona que silencia conjurações e drena vida, sem morte instantânea."));
            Add(new SpellEntry("ama.spell.transmutation.0.pele_de_argila", "Pele de Argila", MagicSchool.Transmutation, 0, 3f, 2f, "melhoria", "ally", "Endurece temporariamente a pele, aumentando levemente armadura."));
            Add(new SpellEntry("ama.spell.transmutation.0.dedos_habeis", "Dedos Hábeis", MagicSchool.Transmutation, 0, 3f, 2f, "melhoria", "ally", "Aumenta velocidade de ataque de uma unidade por pouco tempo."));
            Add(new SpellEntry("ama.spell.transmutation.1.endurecer_aco", "Endurecer Aço", MagicSchool.Transmutation, 1, 10f, 4f, "melhoria", "ally", "Fortalece dano físico de uma arma equipada."));
            Add(new SpellEntry("ama.spell.transmutation.1.pes_de_vento", "Pés de Vento", MagicSchool.Transmutation, 1, 10f, 4f, "melhoria", "ally", "Aumenta velocidade de deslocamento por pouco tempo."));
            Add(new SpellEntry("ama.spell.transmutation.2.garra_de_ferro", "Garra de Ferro", MagicSchool.Transmutation, 2, 18f, 7f, "transmutacao", "ally", "Transforma temporariamente os braços em armas naturais."));
            Add(new SpellEntry("ama.spell.transmutation.2.lama_ondulante", "Lama Ondulante", MagicSchool.Transmutation, 2, 18f, 7f, "barreira", "area", "Cria terreno viscoso temporário que atrasa avanço."));
            Add(new SpellEntry("ama.spell.transmutation.3.corpo_de_basalto", "Corpo de Basalto", MagicSchool.Transmutation, 3, 30f, 11f, "transmutacao", "ally", "Concede armadura considerável e reduz velocidade de movimento."));
            Add(new SpellEntry("ama.spell.transmutation.3.passo_alado", "Passo Alado", MagicSchool.Transmutation, 3, 30f, 11f, "teleporte", "ally", "Realiza um salto curto sobre obstáculo para local seguro."));
            Add(new SpellEntry("ama.spell.transmutation.4.espada_mercurial", "Espada Mercurial", MagicSchool.Transmutation, 4, 45f, 17f, "melhoria", "ally", "Aumenta alcance e fluidez dos próximos golpes de espada."));
            Add(new SpellEntry("ama.spell.transmutation.4.forma_bestial", "Forma Bestial", MagicSchool.Transmutation, 4, 45f, 17f, "transmutacao", "ally", "Concede garras, velocidade e força, com perda temporária de conjuração."));
            Add(new SpellEntry("ama.spell.transmutation.5.armadura_viva", "Armadura Viva", MagicSchool.Transmutation, 5, 65f, 25f, "reflexao", "ally", "Armadura encantada devolve pequena parte do dano físico."));
            Add(new SpellEntry("ama.spell.transmutation.5.campo_de_densidade", "Campo de Densidade", MagicSchool.Transmutation, 5, 65f, 25f, "enfraquecimento", "area", "Aumenta a gravidade sentida por inimigos, reduzindo velocidade."));
            Add(new SpellEntry("ama.spell.transmutation.6.pele_adamantina", "Pele Adamantina", MagicSchool.Transmutation, 6, 90f, 36f, "transmutacao", "ally", "Concede alta armadura com limite de redução de dano."));
            Add(new SpellEntry("ama.spell.transmutation.6.mare_de_pedra", "Maré de Pedra", MagicSchool.Transmutation, 6, 90f, 36f, "barreira", "area", "Ergue obstáculos temporários sem alterar permanentemente os tiles."));
            Add(new SpellEntry("ama.spell.transmutation.7.gigante_de_guerra", "Gigante de Guerra", MagicSchool.Transmutation, 7, 120f, 50f, "transmutacao", "ally", "Aumenta tamanho, vida e força de um aliado por tempo curto."));
            Add(new SpellEntry("ama.spell.transmutation.7.enxame_liquido", "Enxame Líquido", MagicSchool.Transmutation, 7, 120f, 50f, "controle", "area", "Transforma material próximo em ondas lentas que imobilizam inimigos."));
            Add(new SpellEntry("ama.spell.transmutation.8.mestre_das_formas", "Mestre das Formas", MagicSchool.Transmutation, 8, 160f, 75f, "transmutacao", "area", "Transmuta até cinco aliados para papéis marciais especializados."));
            Add(new SpellEntry("ama.spell.transmutation.8.reescrita_da_materia", "Reescrita da Matéria", MagicSchool.Transmutation, 8, 160f, 75f, "enfraquecimento", "area", "Fragiliza fortificações e armaduras numa região, sem editar mapa permanente."));
            Add(new SpellEntry("ama.spell.transmutation.9.avatar_perfeito", "Avatar Perfeito", MagicSchool.Transmutation, 9, 210f, 120f, "transmutacao", "self", "Concede forma guerreira lendária com bônus altos, mas limitados."));
            Add(new SpellEntry("ama.spell.transmutation.9.transmutacao_do_campo", "Transmutação do Campo", MagicSchool.Transmutation, 9, 210f, 120f, "transmutacao", "area", "Fortalece até quinze aliados e atrasa inimigos numa zona ampla."));
        }
        private static void Add(SpellEntry entry) { All.Add(entry); ById[entry.Id]=entry; }
        public static SpellEntry Find(string id)
        { SpellEntry result; return ById.TryGetValue(id, out result) ? result : null; }
        public static List<SpellEntry> School(MagicSchool school)
        {
            List<SpellEntry> results = new List<SpellEntry>();
            foreach (SpellEntry s in All) if (s.School == school) results.Add(s);
            return results;
        }
        // Explicit supported mechanics for this experimental release.
        // Advanced spells still appear in the catalog but can't consume mana yet.
        public static bool HasRuntime(SpellEntry s)
        {
            if (s == null || s.Circle > 1) return false;
            if (s.Category == "dano" || s.Category == "dano_em_area" ||
                s.Category == "dreno_vital" || s.Category == "escudo" ||
                s.Category == "melhoria" || s.Category == "ilusao" ||
                s.Category == "deteccao" || s.Category == "controle" ||
                s.Category == "enfraquecimento") return true;
            return false;
        }
    }
}
