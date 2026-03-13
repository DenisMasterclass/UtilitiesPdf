namespace Docs
{

    public static class CPFValidator
    {
        public static bool IsValid(string cpf)
        {
            // Remove caracteres não numéricos do CPF
            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            // Verifica se o CPF possui 11 dígitos
            if (cpf.Length != 11)
                return false;

            // Verifica se todos os dígitos são iguais (CPF inválido)
            if (cpf.All(digit => digit == cpf[0]))
                return false;

            // Calcula o primeiro dígito verificador
            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += (cpf[i] - '0') * (10 - i);
            }
            int firstDigit = 11 - (sum % 11);
            if (firstDigit >= 10)
            {
                firstDigit = 0;
            }

            // Verifica se o primeiro dígito verificador é igual ao décimo dígito do CPF
            if (firstDigit != cpf[9] - '0')
                return false;

            // Calcula o segundo dígito verificador
            sum = 0;
            for (int i = 0; i < 10; i++)
            {
                sum += (cpf[i] - '0') * (11 - i);
            }
            int secondDigit = 11 - (sum % 11);
            if (secondDigit >= 10)
            {
                secondDigit = 0;
            }

            // Verifica se o segundo dígito verificador é igual ao décimo primeiro dígito do CPF
            return secondDigit == cpf[10] - '0';
        }
    }
    public static class CNPJValidator
    {
        public static bool IsValid(string cnpj)
        {
            // Remove caracteres não numéricos do CNPJ
            cnpj = new string(cnpj.Where(char.IsDigit).ToArray());

            // Verifica se o CNPJ possui 14 dígitos
            if (cnpj.Length != 14)
                return false;

            // Verifica se todos os dígitos são iguais (CNPJ inválido)
            if (cnpj.All(digit => digit == cnpj[0]))
                return false;

            // Calcula o primeiro dígito verificador
            int[] weights1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                sum += (cnpj[i] - '0') * weights1[i];
            }
            int firstDigit = 11 - (sum % 11);
            if (firstDigit >= 10)
            {
                firstDigit = 0;
            }

            // Verifica se o primeiro dígito verificador é igual ao décimo terceiro dígito do CNPJ
            if (firstDigit != cnpj[12] - '0')
                return false;

            // Calcula o segundo dígito verificador
            int[] weights2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            sum = 0;
            for (int i = 0; i < 13; i++)
            {
                sum += (cnpj[i] - '0') * weights2[i];
            }
            int secondDigit = 11 - (sum % 11);
            if (secondDigit >= 10)
            {
                secondDigit = 0;
            }

            // Verifica se o segundo dígito verificador é igual ao décimo quarto dígito do CNPJ
            return secondDigit == cnpj[13] - '0';
        }
    }
}