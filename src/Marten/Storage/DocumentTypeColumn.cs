using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using LamarCodeGeneration;
using Marten.Internal.CodeGeneration;
using Marten.Schema;

namespace Marten.Storage
{
    internal class DocumentTypeColumn: MetadataColumn, ISelectableColumn
    {
        public DocumentTypeColumn(DocumentMapping mapping) : base(DocumentMapping.DocumentTypeColumn, "varchar")
        {
            CanAdd = true;
            Directive = $"DEFAULT '{mapping.AliasFor(mapping.DocumentType)}'";
            mapping.AddIndex(DocumentMapping.DocumentTypeColumn);
        }

        public void GenerateCode(StorageStyle storageStyle, GeneratedType generatedType, GeneratedMethod async,
            GeneratedMethod sync, int index,
            DocumentMapping mapping)
        {
            var member = mapping.DocumentTypeMember;
            var variableName = "docType";
            var memberType = typeof(string);

            if (member == null) return;

            sync.Frames.Code($"var {variableName} = reader.GetFieldValue<{memberType.FullNameInCode()}>({index});");
            async.Frames.CodeAsync($"var {variableName} = await reader.GetFieldValueAsync<{memberType.FullNameInCode()}>({index}, token);");

            sync.Frames.SetMemberValue(member, variableName, mapping.DocumentType, generatedType);
            async.Frames.SetMemberValue(member, variableName, mapping.DocumentType, generatedType);
        }

        public bool ShouldSelect(DocumentMapping mapping, StorageStyle storageStyle)
        {
            return true;
        }

        public override async Task ApplyAsync(DocumentMetadata metadata, int index, DbDataReader reader, CancellationToken token)
        {
            metadata.DocumentType = await reader.GetFieldValueAsync<string>(index, token);
        }

        public override void Apply(DocumentMetadata metadata, int index, DbDataReader reader)
        {
            metadata.DocumentType = reader.GetFieldValue<string>(index);
        }
    }
}
