/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace Hisser.ClientServer
{
    public class PemConverter
    {
        private const int bytemask1 = 255;          //byte ---X
        private const int bytemask2 = 65280;        //byte --X-
        private const int bytemask3 = 16711680;     //byte -X--

        private const byte CRYPT_RSA_ASN1_INTEGER = 2;
        private const byte CRYPT_RSA_ASN1_SEQUENCE = 48;

        public static byte[] ToPem(RSAParameters keyInfo, bool PREFIX_MODULUS_WITH_ZERO = false)
        {
            /*
            eg. CRYPT_RSA_PUBLIC_FORMAT_PKCS1_RAW or CRYPT_RSA_PUBLIC_FORMAT_PKCS1 
            from <http://tools.ietf.org/html/rfc3447#appendix-A.1.1>: 
            RSAPublicKey ::= SEQUENCE { 
                 modulus           INTEGER,  -- n 
                 publicExponent    INTEGER   -- e 
            }
            */

            /*
            $components = array( 
                'modulus' => pack('Ca*a*', CRYPT_RSA_ASN1_INTEGER, $this->_encodeLength(strlen($modulus)), $modulus), 
                'publicExponent' => pack('Ca*a*', CRYPT_RSA_ASN1_INTEGER, $this->_encodeLength(strlen($publicExponent)), $publicExponent) 
            );
            */
            byte[] modulus = PREFIX_MODULUS_WITH_ZERO ? (new byte[] { 0 }).Concat(keyInfo.Modulus).ToArray() : keyInfo.Modulus;
            modulus = pack_C(CRYPT_RSA_ASN1_INTEGER)
                .Concat(pack_a(EncodeLength(modulus.Length)))
                .Concat(pack_a(modulus))
                .ToArray();

            byte[] publicExponent = pack_C(CRYPT_RSA_ASN1_INTEGER)
                .Concat(pack_a(EncodeLength(keyInfo.Exponent.Length)))
                .Concat(pack_a(keyInfo.Exponent))
                .ToArray();

            /*
            (1)
            $RSAPublicKey = pack('Ca*a*a*', 
                CRYPT_RSA_ASN1_SEQUENCE, $this->_encodeLength(strlen($components['modulus']) + strlen($components['publicExponent'])), 
                $components['modulus'], $components['publicExponent'] 
            );
            */
            byte[] RSAPublicKey = pack_C(CRYPT_RSA_ASN1_SEQUENCE)
                .Concat(pack_a(EncodeLength(modulus.Length + publicExponent.Length)))
                .Concat(pack_a(modulus))
                .Concat(pack_a(publicExponent))
                .ToArray();

            /*
            if ($this->publicKeyFormat == CRYPT_RSA_PUBLIC_FORMAT_PKCS1) { 
                // sequence(oid(1.2.840.113549.1.1.1), null)) = rsaEncryption. 
                $rsaOID = pack('H*', '300d06092a864886f70d0101010500'); // hex version of MA0GCSqGSIb3DQEBAQUA 
                $RSAPublicKey = chr(0) . $RSAPublicKey; 
                $RSAPublicKey = chr(3) . $this->_encodeLength(strlen($RSAPublicKey)) . $RSAPublicKey; 
 
                $RSAPublicKey = pack('Ca*a*', 
                    CRYPT_RSA_ASN1_SEQUENCE, $this->_encodeLength(strlen($rsaOID . $RSAPublicKey)), $rsaOID . $RSAPublicKey 
                ); 
            }
            */

            byte[] rsaOID = pack_H("300d06092a864886f70d0101010500");
            RSAPublicKey = (new byte[1] { 0 })
                .Concat(RSAPublicKey).ToArray();
            RSAPublicKey = (new byte[1] { 3 })
                .Concat(EncodeLength(RSAPublicKey.Length))
                .Concat(RSAPublicKey).ToArray();
            byte[] rsaOIDandRSAPublicKey = rsaOID
                .Concat(RSAPublicKey).ToArray();

            RSAPublicKey = pack_C(CRYPT_RSA_ASN1_SEQUENCE)
                .Concat(pack_a(EncodeLength(rsaOIDandRSAPublicKey.Length)))
                .Concat(pack_a(rsaOIDandRSAPublicKey))
                .ToArray();

            /*
            $RSAPublicKey = "-----BEGIN PUBLIC KEY-----\r\n" . 
            chunk_split(base64_encode($RSAPublicKey), 64) . 
            '-----END PUBLIC KEY-----'; 
 
            return $RSAPublicKey; 
            */
            string RSAPublicKey_base64 = Convert.ToBase64String(RSAPublicKey.ToArray());

            StringBuilder output = new StringBuilder();
            output.AppendLine("----------BEGIN PUBLIC KEY----------");
            // Output as Base64 with lines chopped at 64 characters
            for (var i = 0; i < RSAPublicKey_base64.Length; i += 64)
            {
                output.AppendLine(RSAPublicKey_base64.Substring(i, Math.Min(64, RSAPublicKey_base64.Length - i)));
            }
            output.AppendLine("-----END PUBLIC KEY-----");
            return Encoding.UTF8.GetBytes(output.ToString());
        }

        public static RSAParameters FromPem(byte[] pem, bool PREFIX_MODULUS_WITH_ZERO = false)
        {
            /*
            eg. CRYPT_RSA_PUBLIC_FORMAT_PKCS1_RAW or CRYPT_RSA_PUBLIC_FORMAT_PKCS1 
            from <http://tools.ietf.org/html/rfc3447#appendix-A.1.1>: 
            RSAPublicKey ::= SEQUENCE { 
                 modulus           INTEGER,  -- n 
                 publicExponent    INTEGER   -- e 
            }
            */

            byte[] readBytes;
            byte readByte;
            int length;

            byte[] entireEncodedKey;
            using (MemoryStream buffer = new MemoryStream(pem))
            {
                StreamReader input = new StreamReader(buffer);

                //Remove //----------BEGIN PUBLIC KEY----------
                input.ReadLine();

                StringBuilder sb = new StringBuilder();
                while (!input.EndOfStream)
                {
                    sb.Append(input.ReadLine());
                }

                //Remove -----END PUBLIC KEY-----
                sb.Remove(sb.Length - 24, 24);

                entireEncodedKey = Convert.FromBase64String(sb.ToString());
            }

            byte[] rsaOIDandRSAPublicKey;
            using (MemoryStream buffer = new MemoryStream(entireEncodedKey))
            {
                //pack_C(CRYPT_RSA_ASN1_SEQUENCE)
                if ((readByte = (byte)buffer.ReadByte()) != CRYPT_RSA_ASN1_SEQUENCE)
                {
                    throw new TechnicalException(string.Format("Invalid Public Key: {0}, CRYPT_RSA_ASN1_SEQUENCE expected at position {1}.", Convert.ToBase64String(pem), buffer.Position));
                }

                //pack_a(EncodeLength(rsaOIDandRSAPublicKey.Length))
                length = DecodeLength(buffer);

                //pack_a(rsaOIDandRSAPublicKey)
                rsaOIDandRSAPublicKey = new byte[length];
                buffer.Read(rsaOIDandRSAPublicKey, 0, length);
            }

            byte[] RSAPublicKey;
            using (MemoryStream buffer = new MemoryStream(rsaOIDandRSAPublicKey))
            {
                //rsaOID has a fixed length of 15 bytes
                readBytes = new byte[15];
                buffer.Read(readBytes, 0, 15);

                //new byte[1] {3}
                if ((readByte = (byte)buffer.ReadByte()) != 3)
                {
                    throw new TechnicalException(string.Format("Invalid Public Key: {0}, value 3 expected at position {1}.", Convert.ToBase64String(pem), buffer.Position));
                }
                //EncodeLength(RSAPublicKey.Length)
                length = DecodeLength(buffer);

                //new byte[1] { 0 }
                if ((readByte = (byte)buffer.ReadByte()) != 0)
                {
                    throw new TechnicalException(string.Format("Invalid Public Key: {0}, value 0 expected at position {1}.", Convert.ToBase64String(pem), buffer.Position));
                }

                //RSAPublicKey
                RSAPublicKey = new byte[length - 1];
                buffer.Read(RSAPublicKey, 0, length - 1);
            }

            byte[] modulus_and_exponent;
            using (MemoryStream buffer = new MemoryStream(RSAPublicKey))
            {
                //pack_C(CRYPT_RSA_ASN1_SEQUENCE)
                if ((readByte = (byte)buffer.ReadByte()) != CRYPT_RSA_ASN1_SEQUENCE)
                {
                    throw new TechnicalException(string.Format("Invalid Public Key: {0}, CRYPT_RSA_ASN1_SEQUENCE expected at position {1}.", Convert.ToBase64String(pem), buffer.Position));
                }

                //pack_a(EncodeLength(modulus.Length + publicExponent.Length))
                length = DecodeLength(buffer);

                //pack_a(modulus)).Concat(pack_a(publicExponent)
                modulus_and_exponent = new byte[length];
                buffer.Read(modulus_and_exponent, 0, length);
            }

            using (MemoryStream buffer = new MemoryStream(modulus_and_exponent))
            {
                //pack_C(CRYPT_RSA_ASN1_INTEGER)
                if ((readByte = (byte)buffer.ReadByte()) != CRYPT_RSA_ASN1_INTEGER)
                {
                    throw new TechnicalException(string.Format("Invalid Public Key: {0}, CRYPT_RSA_ASN1_INTEGER expected at position {1}.", Convert.ToBase64String(pem), buffer.Position));
                }

                //pack_a(EncodeLength(keyInfo.Modulus.Length))
                length = PREFIX_MODULUS_WITH_ZERO ? DecodeLength(buffer) - 1 : DecodeLength(buffer);

                byte[] modulus = new byte[length];

                if (PREFIX_MODULUS_WITH_ZERO)
                {
                    buffer.ReadByte();
                }

                buffer.Read(modulus, 0, length);

                //pack_C(CRYPT_RSA_ASN1_INTEGER)
                if ((readByte = (byte)buffer.ReadByte()) != CRYPT_RSA_ASN1_INTEGER)
                {
                    throw new TechnicalException(string.Format("Invalid Public Key: {0}, CRYPT_RSA_ASN1_INTEGER expected at position {1}.", Convert.ToBase64String(pem), buffer.Position));
                }

                //pack_a(EncodeLength(keyInfo.Modulus.Length))
                length = DecodeLength(buffer);

                //pack_a(keyInfo.Modulus)
                byte[] exponent = new byte[length];
                buffer.Read(exponent, 0, length);

                RSAParameters result = new RSAParameters();
                result.Modulus = modulus;
                result.Exponent = exponent;
                return result;
            }
        }

        /// <summary>
        /// DER-encode the length 
        /// DER supports lengths up to (2**8)**127, however, we'll only support lengths up to (2**8)**4.  See 
        /// {@link http://itu.int/ITU-T/studygroups/com17/languages/X.690-0207.pdf#p=13 X.690 paragraph 8.1.3} for more information. 
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] EncodeLength(int length)
        {
            /*
            if ($length <= 0x7F) { 
                return chr($length); 
            } 
 
            $temp = ltrim(pack('N', $length), chr(0)); 
            return pack('Ca*', 0x80 | strlen($temp), $temp); 
            */

            if (length <= 0x7F)
            {
                return new byte[] { (byte)length };
            }
            else
            {
                byte[] temp = LTrim(pack_N(length), 0);
                return pack_C((byte)(0x80 | temp.Length)).Concat(pack_a(temp)).ToArray();
            }
        }

        public static int DecodeLength(Stream s)
        {
            /*
            if ($length <= 0x7F) { 
                return chr($length); 
            } 
 
            $temp = ltrim(pack('N', $length), chr(0)); 
            return pack('Ca*', 0x80 | strlen($temp), $temp); 
            */

            int encodedLength = s.ReadByte();
            if (encodedLength <= 0x7F)
            {
                return encodedLength;
            }
            else
            {
                encodedLength -= 0x80;
                byte[] lengthValue = new byte[4];
                s.Read(lengthValue, 4 - encodedLength, encodedLength);
                return (int)unpack_N(lengthValue);
            }
        }

        public static byte[] pack_C(byte value)
        {
            return new byte[] { value };
        }

        public static byte unpack_C(byte[] value)
        {
            return value[0];
        }

        public static byte[] pack_a(byte[] value)
        {
            return value;
        }

        public static byte[] unpack_a(byte[] value)
        {
            return value;
        }

        public static byte[] pack_H(string value)
        {
            byte[] result = new byte[value.Length / 2];
            for (int p = 0; p < value.Length; p += 2)
            {
                string hexValue = value.Substring(p, 2);
                result[p / 2] = (byte)Convert.ToInt32(hexValue, 16);
            }
            return result;
        }

        public static string unpack_H(byte[] value)
        {
            string result = "";
            for (int i = 0; i < value.Length; i++)
            {
                result += Convert.ToString(value[i], 16);
            }
            return result;
        }

        public static byte[] pack_N(long value)
        {
            byte[] result = new byte[4];
            result[0] = (byte)(value >> 24);
            result[1] = (byte)((value & bytemask3) >> 16);
            result[2] = (byte)((value & bytemask2) >> 8);
            result[3] = (byte)(value & bytemask1);
            return result;
        }

        public static long unpack_N(byte[] value)
        {
            return value[0] << 24 | value[1] << 16 | value[2] << 8 | value[3];
        }

        /// <summary>
        /// Copies PHP's ltrim functionality
        /// </summary>
        /// <param name="trimmed"></param>
        /// <param name="trimchar"></param>
        /// <returns></returns>
        public static byte[] LTrim(byte[] trimmed, byte trimchar)
        {
            int i = 0;
            while (i < trimmed.Length && trimmed[i] == trimchar)
            {
                i++;
            }

            if (i == 0)
            {
                return trimmed;
            }
            else
            {
                byte[] result = new byte[trimmed.Length - i];
                Array.Copy(trimmed, i, result, 0, result.Length);
                return result;
            }
        }

        public static List<Tuple<char, int>> DetermineFormatElements(string format)
        {

            List<Tuple<char, int>> result = new List<Tuple<char, int>>();

            for (int i = 0; i < format.Length; i++)
            {
                char command = format[i];
                int repeaterSize = 0;
                int repeater = DetermineRepeater(format, i + 1, out repeaterSize);
                result.Add(new Tuple<char, int>(command, repeater));
                i += repeaterSize;
            }

            return result;
        }

        public static int DetermineRepeater(string format, int startIndex, out int repeaterSize)
        {
            if (startIndex >= format.Length)
            {
                repeaterSize = 0;
                return 1;
            }
            else if (format[startIndex] == '*')
            {
                repeaterSize = 1;
                return Int32.MaxValue;
            }
            else
            {
                //Parse a number
                int i = 0;
                int result = 1;
                int newResult = -1;
                while (Int32.TryParse(format.Substring(startIndex, i), out newResult))
                {
                    result = newResult;
                    i++;
                }
                repeaterSize = i;
                return result;
            }
        }
    }
}
