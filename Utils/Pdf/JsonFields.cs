using System.Text.Json;

namespace Utils.Pdf
{
    public static class JsonFields
    {
        public static void PercorrerElemento(JsonElement elemento)
        {
            // Caso 1: O elemento é um Objeto (ex: { "chave": "valor" })
            if (elemento.ValueKind == JsonValueKind.Object)
            {
                // EnumerateObject garante a ordem original do arquivo
                foreach (JsonProperty propriedade in elemento.EnumerateObject())
                {
                    //Console.WriteLine($"Campo encontrado: {propriedade.Name}");

                    // Chamada recursiva para verificar o valor desta propriedade
                    // (caso o valor seja outro objeto ou array aninhado)
                    PercorrerElemento(propriedade.Value);
                }
            }
            // Caso 2: O elemento é um Array (ex: [ ... ])
            else if (elemento.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in elemento.EnumerateArray())
                {
                    // Arrays não têm "nomes" próprios, mas precisamos
                    // percorrer seus itens para achar campos dentro deles
                    PercorrerElemento(item);
                }
            }
            // Caso 3: Valores simples (String, Number, True/False, Null)
            else
            {
                // Chegamos ao fim de um ramo (folha), não há mais campos aqui.
                return;
            }
        }
    }
}
