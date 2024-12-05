using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RoubaMonte
{
    public class Carta
    {
        public int Numero { get; set; }
        public string Naipe { get; set; }

        public Carta(int numero, string naipe)
        {
            Numero = numero;
            Naipe = naipe;
        }

        public string DescreverCarta() // Para na hora de chamar os numeros aparecerem como AS,REi, etc
        {
            if (Numero == 1)
            {
                return $"Ás de {Naipe}";
            }
            else if (Numero > 1 && Numero <= 10)
            {
                return $"{Numero} de {Naipe}";
            }
            else if (Numero == 11)
            {
                return $"Valete de {Naipe}";
            }
            else if (Numero == 12)
            {
                return $"Dama de {Naipe}";
            }
            else if (Numero == 13)
            {
                return $"Rei de {Naipe}";
            }
            else
            {
                return $"Carta inválida ({Numero} de {Naipe})";
            }
        }
    

}

    public class Jogador
    {
        public string Nome { get; set; }
        public int Posicao { get; set; }
        public int QuantidadeCartas;
        public Stack<Carta> Monte { get; private set; } = new Stack<Carta>();
  

        public void AtualizarQuantidadeDeCartas()// UM metodo qe fiz para contar a quantidade de cartas a toda rodada dos jogadores e ir atualizando o rakimg mas ainda não fiz o ranking 
        {
            this.QuantidadeCartas = Monte.Count;
        }

        public Jogador(string nome)
        {
            Nome = nome;
        }



        public string DescreverJogador()//So e usado no final para aparecer os rankings 
        {
            return Posicao + "º lugar: " + Nome + " com " + QuantidadeCartas + " cartas.";
        }

    }

    public class Jogo
    {
        private List<Jogador> jogadores = new List<Jogador>();
        private Stack<Carta> monteCompra = new Stack<Carta>();
        private List<Carta> areaDescarte = new List<Carta>();
        private int quantidadeBaralhos; 
        private int quantidadeJogadores;
        private string logFilePath = "log_partida.txt";

        public Jogo(int quantidadeJogadores, int quantidadeBaralhos)// Inicializar jogo 
        {
            this.quantidadeJogadores = quantidadeJogadores;
            this.quantidadeBaralhos = quantidadeBaralhos;
        }


        private void GerarBaralho()
        {
            for (int i = 0; i < quantidadeBaralhos; i++)
            {
                for (int j = 1; j <= 13; j++)
                {
                    string[] naipes = { "Copas", "Espadas", "Ouros", "Paus" };
                    for (int k = 0; k < naipes.Length; k++)
                    {
                        monteCompra.Push(new Carta(j, naipes[k]));
                    }
                }
            }
        }

        private void EmbaralharCartas()
        {

            List<Carta> temp = new List<Carta>();

            foreach(Carta carta in monteCompra) { temp.Add(carta); }
            monteCompra.Clear();

            Random RandomizarCartas = new Random();
            // Cruia uma lista coiloca as cartas nela e faz um randon do tamanho da lista e a cada saida retira 1 deste tamanho 

            while (temp.Count > 0)
            {
                int index = RandomizarCartas.Next(temp.Count);
                monteCompra.Push(temp[index]);
                temp.RemoveAt(index);
            }

        }


        public void Iniciar()
        {
            // Adiciona os jogadores e coloca nome 

            for (int i = 1; i <= quantidadeJogadores; i++)
            {
                Console.Write($"Nome do jogador {i}: ");
                string nome = Console.ReadLine();
                jogadores.Add(new Jogador(nome));
            }

            GerarBaralho();
            EmbaralharCartas();

            JogarPartida();

        }

        private void ExibirEstadoAtual(Jogador jogador, Carta cartaDaVez)//Metodo para exibir o estado do jogador que esta jogando 
        {
            Console.Clear();
            Console.WriteLine($"Jogador atual: {jogador.Nome}");

            Console.Write($"Monte de {jogador.Nome}: ");
            foreach (var carta in jogador.Monte)
            {
                Console.Write($"{carta.DescreverCarta()}, ");
            }
            Console.WriteLine(); 

            Console.WriteLine($"Carta da vez: {cartaDaVez.DescreverCarta()}");

            Console.Write("Área de descarte: ");
            foreach (var carta in areaDescarte)
            {
                Console.Write($"{carta.DescreverCarta()}, ");
            }

            Console.WriteLine();
            Console.WriteLine("Carta no topo de outros jogadores:");
            MostrarCartasTopo(jogador);

            Console.WriteLine(); 


            Console.WriteLine("\nPressione Enter para continuar...");
            Console.ReadLine();
        }


        private void JogarPartida()//Iniciar partida com o log de partidas para salvar em um arquivio 
        {
            using (var writer = new StreamWriter(logFilePath))
            {
                writer.WriteLine($"Partida iniciada com {jogadores.Count} jogadores e {monteCompra.Count} cartas no monte de compra.");
                writer.Write("Jogadores: ");
                foreach (var jogador in jogadores)
                {
                    writer.Write(jogador.Nome);
                    if (jogador != jogadores.Last()) // Adiciona uma vírgula apenas se não for o último jogador
                    {
                        writer.Write(", ");
                    }
                }
                writer.WriteLine(); // Finaliza a linha

                int jogadorAtual = 0;
                while (monteCompra.Count > 0) // Aqui e como se iniciase mesmo fica nesse whilw ate o monte de compras acabar ou seja o jogo acaba 
                {
                    Jogador jogador = jogadores[jogadorAtual];
                    Carta cartaDaVez = monteCompra.Pop();

                    // Exibe cartas no topo do jogador 
                    MostrarCartasTopo(jogador);

                    // Se o jogador não tiver nenhuma carta pega a carta da vez e pula para o proximo so adicionando a carta para o jogadpr 
                    if (jogador.Monte.Count == 0)
                    {
                        jogador.Monte.Push(cartaDaVez);
                        Console.WriteLine($"{jogador.Nome} não tinha cartas. Pegou a carta {cartaDaVez.DescreverCarta()} e passou a vez.");
                        writer.WriteLine($"{jogador.Nome} não tinha cartas. Pegou a carta {cartaDaVez.DescreverCarta()} e passou a vez.");

                        // Passa para o próximo jogador
                        jogadorAtual = (jogadorAtual + 1) % jogadores.Count;
                    }
                    else
                    {
                        // Exibe estado atual
                        ExibirEstadoAtual(jogador, cartaDaVez);
                        writer.WriteLine($"{jogador.Nome} retirou a carta {cartaDaVez}.");

                        // Processa a jogada
                        if (!ProcessarJogada(jogador, cartaDaVez, writer))// processa a jogado e se o valor vier false entra no if para descartas 
                        {
                            areaDescarte.Add(cartaDaVez);
                            writer.WriteLine($"{jogador.Nome} descartou a carta {cartaDaVez}.");
                        }

                        // Passa para o próximo jogador
                        jogadorAtual = (jogadorAtual + 1) % jogadores.Count;
                    }
                }

                DeterminarVencedores(writer);
            }
        }



        private void MostrarCartasTopo(Jogador jogadorAtual)
        {
           
            Console.Write($"Carta no topo do monte de {jogadorAtual.Nome}: ");
            if (jogadorAtual.Monte.Count != 0)
            {
                Console.WriteLine(jogadorAtual.Monte.Peek().DescreverCarta());
            }
            else
            {
                Console.WriteLine("Nenhuma carta");
            }

            foreach (var jogador in jogadores)
            {
                if (jogador != jogadorAtual)
                {
                    Console.Write($"Carta no topo do monte de {jogador.Nome}: ");
                    if (jogador.Monte.Count != 0)
                    {
                        Console.WriteLine(jogador.Monte.Peek().DescreverCarta());
                    }
                    else
                    {
                        Console.WriteLine("Nenhuma carta");
                    }
                }
            }

            Console.WriteLine("\nPressione Enter para continuar...");
            Console.ReadLine();
        }



        private void DeterminarVencedores(StreamWriter writer)
        {
            int maiorMonte = 0;
            List<Jogador> vencedores = new List<Jogador>();

            // Determina o maior número de cartas e os vencedores
            foreach (var jogador in jogadores)
            {
                jogador.AtualizarQuantidadeDeCartas();

                if (jogador.QuantidadeCartas > maiorMonte)
                {
                    maiorMonte = jogador.QuantidadeCartas;
                    vencedores.Clear(); // Limpa a lista, pois há um novo maior monte
                    vencedores.Add(jogador);
                }
                else if (jogador.QuantidadeCartas == maiorMonte)
                {
                    vencedores.Add(jogador);
                }
            }

            // Escreve os vencedores no arquivo
            writer.WriteLine("Resultado da partida:");
            foreach (var vencedor in vencedores)
            {
                writer.WriteLine($"{vencedor.Nome} venceu com {vencedor.QuantidadeCartas} cartas.");
            }

            // Organiza os jogadores em ordem decrescente e exibe o ranking final
            for (int i = 0; i < jogadores.Count - 1; i++)
            {
                for (int j = i + 1; j < jogadores.Count; j++)
                {
                    if (jogadores[i].QuantidadeCartas < jogadores[j].QuantidadeCartas)
                    {
                        // Troca de posição
                        var temp = jogadores[i];
                        jogadores[i] = jogadores[j];
                        jogadores[j] = temp;
                    }
                }
            }

            Console.WriteLine("Ranking Final:");
            for (int i = 0; i < jogadores.Count; i++)
            {
                jogadores[i].Posicao = i + 1;
                Console.WriteLine(jogadores[i].DescreverJogador());
            }
        }

    private bool ProcessarJogada(Jogador jogador, Carta cartaDaVez, StreamWriter writer)
        {
            // 1. Verifica se pode roubar o monte de outro jogador
            Jogador monteParaRoubar = null;
            int maiorMonte = -1;

            foreach (var j in jogadores)
            {
                // Verifica se não é o jogador atual e se o monte do jogador tem cartas
                if (j != jogador && j.Monte.Count != 0)
                {
                    // Verifica se a carta no topo do monte tem o mesmo número da carta da vez
                    if (j.Monte.Peek().Numero == cartaDaVez.Numero)
                    {
                        // Se tiver salva e verifica os outros so ira passar aquele que tiver combinando e com o maior monte 
                        if (j.Monte.Count > maiorMonte)
                        {
                            monteParaRoubar = j;
                            maiorMonte = j.Monte.Count;
                        }
                    }
                }
            }

            if (monteParaRoubar != null)//Entra se tiver algum monte que o jogador pode roubar 
            {
                while (monteParaRoubar.Monte.Count == 0)
                {
                    jogador.Monte.Push(monteParaRoubar.Monte.Pop());
                }
                jogador.Monte.Push(cartaDaVez);
                Console.WriteLine($"{jogador.Nome} roubou o monte de {monteParaRoubar.Nome}!");
                jogador.AtualizarQuantidadeDeCartas();
                writer.WriteLine($"{jogador.Nome} roubou o monte de {monteParaRoubar.Nome}.");
                return true;
            }

            // 2. Verifica se a carta está na área de descarte
            Carta cartaDescarte = null;
            foreach (var carta in areaDescarte)
            {
                if (carta.Numero == cartaDaVez.Numero)
                {
                    cartaDescarte = carta;
                    break;//Carta estando na area de descartas sai 
                }
            }
            if (cartaDescarte != null) // entra nesse if se o jogador a carta da ves estiver na area de descarte e pega ela retornando true 
            {
                jogador.Monte.Push(cartaDescarte);
                areaDescarte.Remove(cartaDescarte);
                jogador.Monte.Push(cartaDaVez);
                jogador.AtualizarQuantidadeDeCartas();
                Console.WriteLine($"{jogador.Nome} pegou a carta {cartaDescarte.DescreverCarta()} da área de descarte.");
                return true;
            }

            //  Verifica se a carta é igual ao topo do próprio monte
            if (jogador.Monte.Count == 0 && jogador.Monte.Peek().Numero == cartaDaVez.Numero)
            {
                jogador.Monte.Push(cartaDaVez);
                jogador.AtualizarQuantidadeDeCartas();
                Console.WriteLine($"{jogador.Nome} adicionou a carta {cartaDaVez} ao próprio monte.");
                writer.WriteLine($"{jogador.Nome} adicionou a carta {cartaDaVez} ao próprio monte.");
                return true;
            }

            return false;
        }

      


        class Program
        {
            static void Main()
            {
                bool continuarJogo = true;

                while (continuarJogo)
                {
                    Console.WriteLine("Quantos jogadores participarão?");
                    int quantidadeJogadores = int.Parse(Console.ReadLine());
                   
                    while (quantidadeJogadores <= 0)
                    {
                        Console.WriteLine("Por favor, insira um número válido de jogadores (maior que 0).");
                        quantidadeJogadores = int.Parse(Console.ReadLine());
                    }

                    Console.WriteLine("Quantos baralhos serão usados?");
                    int quantidadeBaralhos= int.Parse(Console.ReadLine());
                    while (quantidadeBaralhos <= 0)
                    {
                        Console.WriteLine("Por favor, insira um número válido de baralhos (maior que 0).");
                        quantidadeBaralhos = int.Parse(Console.ReadLine());
                    }

                    Jogo jogo = new Jogo(quantidadeJogadores, quantidadeBaralhos);
                    jogo.Iniciar();


                    Console.WriteLine("Deseja iniciar um novo jogo? (s/n)");
                    string resposta = Console.ReadLine();
                    if(resposta != "s")
                        continuarJogo=false;
                }


            }
        }
    }
}
