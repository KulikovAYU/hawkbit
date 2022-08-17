using System;
using System.Linq;
using System.Text;

namespace ForteConfigurationLoader.SerializationLayer
{
    public class FBDKASN1ComSerializer : BaseSerializer
    {
        public FBDKASN1ComSerializer(IConsumer consumer) : base(consumer)
        {
        }

        public override byte[] Serialize(TagIdentify dataType, byte[] payload, string sResIdent)
        {
            if (TagIdentify.e_STRING == dataType)
                return SerializeAsString(payload, sResIdent);

            return  Array.Empty<byte>();
        }

        public override byte[] Serialize(TagIdentify dataType, object payload, string sResIdent)
        {
            if (payload is string sPayload)
            {
                return Serialize(dataType, Encoding.ASCII.GetBytes(sPayload), sResIdent);
            }

            return  Array.Empty<byte>();
        }

        public override void Deserialize(byte[] payload)
        {
            if(payload.Length == 0)
                return;
            
            int nIter = 0;
            while (nIter != payload.Length)
            {
                byte type = payload[nIter];
                //let's convert type to back:
                //erase not important bits: N6, N7
                EDataTypeTags appType = (EDataTypeTags)(type &
                                                        ~(byte)EASN1TagClass.e_APPLICATION &
                                                        ~(byte)EASN1EncodingType.e_CONSTRUCTED);

                int lenghtChunk1 = (payload[++nIter] & 0x00FF) << 8;
                int lenghtChunk2 = (payload[++nIter] & 0x00FF);

                int payloadLenght = lenghtChunk1 | lenghtChunk2;

                Consumer.OnRecieve(appType, payload.Skip(nIter + 1).Take(payloadLenght).ToArray());

                nIter += (payloadLenght + 1);
            }
        }

        private byte[] SerializeAsString(byte[] payload, string sResIdent)
        {
            const int SERVICE_INFORMATION_BYTES_CHUNKS = 6; //counting of service data
            int nResIdentLength = sResIdent.Length;
            int nLength = payload.Length;
            int totalAllocatedBytes = SERVICE_INFORMATION_BYTES_CHUNKS + nResIdentLength + nLength;

            var data = new byte[totalAllocatedBytes];

            //ATTENTION! Message structure represents like that: [Tag][sResIdent][Tag][payload data]
            byte tagCodeIdent = GetTagCode(TagIdentify.e_STRING); //write Tag

            int nCounting = 0;
            data[nCounting] = tagCodeIdent; //write Tag
            data[++nCounting] = (byte)((nResIdentLength >> 8) & 0x00FF); //write 1 part of length
            data[++nCounting] = (byte)(nResIdentLength & 0x00FF); //write 2 part of length
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(sResIdent), 0, data, nCounting + 1, nResIdentLength);//copy payload

            nCounting += nResIdentLength;
            data[++nCounting] = tagCodeIdent; //write Tag
            data[++nCounting] = (byte)((nLength >> 8) & 0x00FF);//write 1 part of length
            data[++nCounting] = (byte)(nLength & 0x00FF);//write 2 part of length
            Buffer.BlockCopy(payload, 0, data, nCounting + 1 , nLength);//copy payload

            return data;
        }

        private static byte GetTagCode(TagIdentify identify)
        {
            byte[,] csmADataTags = {
            //!< {Tag, Size of data + tag size}; Size == 255 means unknown
                    //TODO: consider size=0 for unknown
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_ANY_TAG, 255 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_BOOL_TAG, 1 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_SINT_TAG, 2 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_INT_TAG, 3 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_DINT_TAG, 5 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_LINT_TAG, 9 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_USINT_TAG, 2 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_UINT_TAG, 3 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_UDINT_TAG, 5 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_ULINT_TAG, 9 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_BYTE_TAG, 2 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_WORD_TAG, 3 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_DWORD_TAG, 5 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_LWORD_TAG, 9 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_DATE_TAG, 9 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_TIME_OF_DAY_TAG, 9 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_DATE_AND_TIME_TAG, 9 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_TIME_TAG, 9 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_REAL_TAG, 5 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_LREAL_TAG, 9 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_STRING_TAG, 255 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_PRIMITIVE   + (byte)EDataTypeTags.e_WSTRING_TAG, 255 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_CONSTRUCTED + (byte)EDataTypeTags.e_DerivedData_TAG, 255 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_CONSTRUCTED + (byte)EDataTypeTags.e_DirectlyDerivedData_TAG, 255 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_CONSTRUCTED + (byte)EDataTypeTags.e_EnumeratedData_TAG, 255 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_CONSTRUCTED + (byte)EDataTypeTags.e_SubrangeData_TAG, 255 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_CONSTRUCTED + (byte)EDataTypeTags.e_ARRAY_TAG, 255 },
                { (byte)EASN1TagClass.e_APPLICATION + (byte)EASN1EncodingType.e_CONSTRUCTED + (byte)EDataTypeTags.e_STRUCT_TAG, 255 } };


            //ATTENTION! The second index must be 0 not 1
            //because we really know size. If we set argument
            //as 1 we will get res = 255 and it will be uknown size
            var res = csmADataTags[(int)identify, 0];
            return res;
        }
    }
}