using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace NetCoreCommonLibrary.Util
{
    /// <summary>
    /// Extensões para manipulação de strings
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Trunca uma string para o tamanho máximo especificado, adicionando um sufixo.
        /// </summary>
        /// <param name="value">A string a ser truncada.</param>
        /// <param name="maxLength">O comprimento máximo desejado (incluindo o sufixo).</param>
        /// <param name="suffix">O sufixo a ser adicionado se a string for truncada (padrão "...").</param>
        /// <returns>A string truncada ou a string original se for menor ou igual ao maxLength.</returns>
        public static string Truncate(this string? value, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
                return value ?? string.Empty;

            if (maxLength <= suffix.Length)
                return suffix.Substring(0, Math.Min(suffix.Length, maxLength)); // Retorna parte do sufixo se maxLength for muito pequeno

            return value.Substring(0, maxLength - suffix.Length) + suffix;
        }

        /// <summary>
        /// Remove acentos de uma string.
        /// </summary>
        /// <param name="text">O texto com acentos.</param>
        /// <returns>O texto sem acentos.</returns>
        public static string RemoveAccents(this string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text ?? string.Empty;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(normalizedString.Length); // Otimiza capacidade inicial

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Retorna a string normalizada de volta para FormC se necessário, ou a string construída.
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Converte uma string para slug (URL amigável).
        /// Remove acentos, converte para minúsculas, substitui espaços e caracteres não alfanuméricos por hífens.
        /// </summary>
        /// <param name="text">O texto a ser convertido.</param>
        /// <returns>A string no formato slug.</returns>
        public static string ToSlug(this string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text ?? string.Empty;

            // Remove acentos e converte para minúsculas
            var slug = text.RemoveAccents().ToLowerInvariant(); // Usa InvariantCulture para consistência
            
            // Substitui caracteres não alfanuméricos (exceto hífen) por nada
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            
            // Substitui espaços e sequências de hífens por um único hífen
            slug = Regex.Replace(slug, @"[\s-]+", "-");
            
            // Remove hífens do início e do fim
            slug = slug.Trim('-');
            
            return slug;
        }

        /// <summary>
        /// Formata um número de CNPJ (XX.XXX.XXX/XXXX-XX).
        /// </summary>
        /// <param name="cnpj">O CNPJ (somente números).</param>
        /// <returns>O CNPJ formatado ou o valor original se inválido.</returns>
        public static string FormatCNPJ(this string? cnpj)
        {
            if (string.IsNullOrEmpty(cnpj))
                return cnpj ?? string.Empty;

            // Remove caracteres não numéricos
            var cleanedCnpj = Regex.Replace(cnpj, @"\D", "");
            
            if (cleanedCnpj.Length != 14)
                return cnpj; // Retorna original se não tiver 14 dígitos

            // Aplica a máscara
            return Convert.ToUInt64(cleanedCnpj).ToString(@"00\.000\.000\/0000\-00");
        }

        /// <summary>
        /// Formata um número de CPF (XXX.XXX.XXX-XX).
        /// </summary>
        /// <param name="cpf">O CPF (somente números).</param>
        /// <returns>O CPF formatado ou o valor original se inválido.</returns>
        public static string FormatCPF(this string? cpf)
        {
            if (string.IsNullOrEmpty(cpf))
                return cpf ?? string.Empty;

            // Remove caracteres não numéricos
            var cleanedCpf = Regex.Replace(cpf, @"\D", "");
            
            if (cleanedCpf.Length != 11)
                return cpf; // Retorna original se não tiver 11 dígitos
            
            // Aplica a máscara
            return Convert.ToUInt64(cleanedCpf).ToString(@"000\.000\.000\-00");
        }

        /// <summary>
        /// Formata um número de telefone (com DDD e 8 ou 9 dígitos).
        /// Formatos: (XX) XXXX-XXXX ou (XX) XXXXX-XXXX.
        /// </summary>
        /// <param name="phone">O número de telefone (somente números).</param>
        /// <returns>O telefone formatado ou o valor original se inválido.</returns>
        public static string FormatPhone(this string? phone)
        {
            if (string.IsNullOrEmpty(phone))
                return phone ?? string.Empty;

            // Remove caracteres não numéricos
            var cleanedPhone = Regex.Replace(phone, @"\D", "");
            
            return cleanedPhone.Length switch
            {
                11 => Convert.ToUInt64(cleanedPhone).ToString(@"(00) 00000\-0000"),
                10 => Convert.ToUInt64(cleanedPhone).ToString(@"(00) 0000\-0000"),
                 9 => Convert.ToUInt64(cleanedPhone).ToString(@"00000\-0000"), // Sem DDD
                 8 => Convert.ToUInt64(cleanedPhone).ToString(@"0000\-0000"),   // Sem DDD
                _ => phone // Retorna original se não tiver 8, 9, 10 ou 11 dígitos
            };
        }

        /// <summary>
        /// Formata um CEP (XXXXX-XXX).
        /// </summary>
        /// <param name="zipCode">O CEP (somente números).</param>
        /// <returns>O CEP formatado ou o valor original se inválido.</returns>
        public static string FormatZipCode(this string? zipCode)
        {
            if (string.IsNullOrEmpty(zipCode))
                return zipCode ?? string.Empty;

            // Remove caracteres não numéricos
            var cleanedZipCode = Regex.Replace(zipCode, @"\D", "");
            
            if (cleanedZipCode.Length != 8)
                return zipCode; // Retorna original se não tiver 8 dígitos

            // Aplica a máscara
            return Convert.ToUInt32(cleanedZipCode).ToString("00000-000");
        }

        /// <summary>
        /// Verifica se uma string contém outra, ignorando acentos e capitalização.
        /// </summary>
        /// <param name="source">A string fonte onde a busca será feita.</param>
        /// <param name="value">O valor a ser procurado dentro da string fonte.</param>
        /// <returns>True se a string fonte contém o valor, False caso contrário.</returns>
        public static bool ContainsIgnoringAccentsAndCase(this string? source, string? value)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value))
                return false;
            
            // Compara usando opções que ignoram acentos e capitalização da cultura corrente
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(source, value, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
        }

        /// <summary>
        /// Converte a primeira letra de uma string para maiúscula (Title Case para a primeira letra).
        /// </summary>
        /// <param name="input">A string de entrada.</param>
        /// <returns>A string com a primeira letra em maiúscula.</returns>
        public static string FirstCharToUpper(this string? input)
        {
            return input switch
            {
                null => string.Empty,
                "" => string.Empty,
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };
        }

        /// <summary>
        /// Converte uma string para o formato Title Case (primeira letra de cada palavra em maiúscula), 
        /// usando as regras da cultura corrente.
        /// </summary>
        /// <param name="input">A string de entrada.</param>
        /// <returns>A string em Title Case.</returns>
        public static string ToTitleCase(this string? input)
        {
            if (string.IsNullOrEmpty(input))
                return input ?? string.Empty;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLowerInvariant()); // Usa ToLowerInvariant para normalizar antes
        }
    }
} 