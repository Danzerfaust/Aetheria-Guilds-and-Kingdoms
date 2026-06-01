using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using SRGuildsAndKingdoms.src.network;

namespace SRGuildsAndKingdoms.src.quests
{
	// Token: 0x0200001C RID: 28
	[NullableContext(1)]
	[Nullable(0)]
	[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonSerializable(typeof(QuestRequirementsJson))]
	[JsonSerializable(typeof(QuestRewardsJson))]
	[JsonSerializable(typeof(QuestProgressJson))]
	[JsonSerializable(typeof(Quest))]
	[JsonSerializable(typeof(List<QuestObjective>))]
	[JsonSerializable(typeof(List<QuestReward>))]
	[GeneratedCode("System.Text.Json.SourceGeneration", "8.0.14.11203")]
	public class QuestJsonContext : JsonSerializerContext, IJsonTypeInfoResolver
	{
		// Token: 0x17000046 RID: 70
		// (get) Token: 0x06000137 RID: 311 RVA: 0x0000CBB0 File Offset: 0x0000ADB0
		public JsonTypeInfo<bool> Boolean
		{
			get
			{
				JsonTypeInfo<bool> result;
				if ((result = this._Boolean) == null)
				{
					result = (this._Boolean = (JsonTypeInfo<bool>)base.Options.GetTypeInfo(typeof(bool)));
				}
				return result;
			}
		}

		// Token: 0x06000138 RID: 312 RVA: 0x0000CBEC File Offset: 0x0000ADEC
		private JsonTypeInfo<bool> Create_Boolean(JsonSerializerOptions options)
		{
			JsonTypeInfo<bool> jsonTypeInfo;
			if (!QuestJsonContext.TryGetTypeInfoForRuntimeCustomConverter<bool>(options, out jsonTypeInfo))
			{
				jsonTypeInfo = JsonMetadataServices.CreateValueInfo<bool>(options, JsonMetadataServices.BooleanConverter);
			}
			jsonTypeInfo.OriginatingResolver = this;
			return jsonTypeInfo;
		}

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x06000139 RID: 313 RVA: 0x0000CC18 File Offset: 0x0000AE18
		public JsonTypeInfo<QuestAcceptedItemDto> QuestAcceptedItemDto
		{
			get
			{
				JsonTypeInfo<QuestAcceptedItemDto> result;
				if ((result = this._QuestAcceptedItemDto) == null)
				{
					result = (this._QuestAcceptedItemDto = (JsonTypeInfo<QuestAcceptedItemDto>)base.Options.GetTypeInfo(typeof(QuestAcceptedItemDto)));
				}
				return result;
			}
		}

		// Token: 0x0600013A RID: 314 RVA: 0x0000CC54 File Offset: 0x0000AE54
		private JsonTypeInfo<QuestAcceptedItemDto> Create_QuestAcceptedItemDto(JsonSerializerOptions options)
		{
			JsonTypeInfo<QuestAcceptedItemDto> jsonTypeInfo;
			if (!QuestJsonContext.TryGetTypeInfoForRuntimeCustomConverter<QuestAcceptedItemDto>(options, out jsonTypeInfo))
			{
				JsonObjectInfoValues<QuestAcceptedItemDto> jsonObjectInfoValues = new JsonObjectInfoValues<QuestAcceptedItemDto>();
				jsonObjectInfoValues.ObjectCreator = (() => new QuestAcceptedItemDto());
				jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = null;
				jsonObjectInfoValues.PropertyMetadataInitializer = ((JsonSerializerContext _) => QuestJsonContext.QuestAcceptedItemDtoPropInit(options));
				jsonObjectInfoValues.ConstructorParameterMetadataInitializer = null;
				jsonObjectInfoValues.SerializeHandler = new Action<Utf8JsonWriter, QuestAcceptedItemDto>(this.QuestAcceptedItemDtoSerializeHandler);
				JsonObjectInfoValues<QuestAcceptedItemDto> objectInfo = jsonObjectInfoValues;
				jsonTypeInfo = JsonMetadataServices.CreateObjectInfo<QuestAcceptedItemDto>(options, objectInfo);
				jsonTypeInfo.NumberHandling = null;
			}
			jsonTypeInfo.OriginatingResolver = this;
			return jsonTypeInfo;
		}

		// Token: 0x0600013B RID: 315 RVA: 0x0000CD00 File Offset: 0x0000AF00
		private static JsonPropertyInfo[] QuestAcceptedItemDtoPropInit(JsonSerializerOptions options)
		{
			JsonPropertyInfo[] array = new JsonPropertyInfo[2];
			JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
			jsonPropertyInfoValues.IsProperty = true;
			jsonPropertyInfoValues.IsPublic = true;
			jsonPropertyInfoValues.IsVirtual = false;
			jsonPropertyInfoValues.DeclaringType = typeof(QuestAcceptedItemDto);
			jsonPropertyInfoValues.Converter = null;
			jsonPropertyInfoValues.Getter = ((object obj) => ((QuestAcceptedItemDto)obj).Code);
			jsonPropertyInfoValues.Setter = delegate(object obj, [Nullable(2)] string value)
			{
				((QuestAcceptedItemDto)obj).Code = value;
			};
			jsonPropertyInfoValues.IgnoreCondition = null;
			jsonPropertyInfoValues.HasJsonInclude = false;
			jsonPropertyInfoValues.IsExtensionData = false;
			jsonPropertyInfoValues.NumberHandling = null;
			jsonPropertyInfoValues.PropertyName = "Code";
			jsonPropertyInfoValues.JsonPropertyName = null;
			JsonPropertyInfoValues<string> info0 = jsonPropertyInfoValues;
			array[0] = JsonMetadataServices.CreatePropertyInfo<string>(options, info0);
			JsonPropertyInfoValues<string> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<string>();
			jsonPropertyInfoValues2.IsProperty = true;
			jsonPropertyInfoValues2.IsPublic = true;
			jsonPropertyInfoValues2.IsVirtual = false;
			jsonPropertyInfoValues2.DeclaringType = typeof(QuestAcceptedItemDto);
			jsonPropertyInfoValues2.Converter = null;
			jsonPropertyInfoValues2.Getter = ((object obj) => ((QuestAcceptedItemDto)obj).Nbt);
			jsonPropertyInfoValues2.Setter = delegate(object obj, [Nullable(2)] string value)
			{
				((QuestAcceptedItemDto)obj).Nbt = value;
			};
			jsonPropertyInfoValues2.IgnoreCondition = null;
			jsonPropertyInfoValues2.HasJsonInclude = false;
			jsonPropertyInfoValues2.IsExtensionData = false;
			jsonPropertyInfoValues2.NumberHandling = null;
			jsonPropertyInfoValues2.PropertyName = "Nbt";
			jsonPropertyInfoValues2.JsonPropertyName = null;
			JsonPropertyInfoValues<string> info = jsonPropertyInfoValues2;
			array[1] = JsonMetadataServices.CreatePropertyInfo<string>(options, info);
			return array;
		}

		// Token: 0x0600013C RID: 316 RVA: 0x0000CE9C File Offset: 0x0000B09C
		private void QuestAcceptedItemDtoSerializeHandler(Utf8JsonWriter writer, [Nullable(2)] QuestAcceptedItemDto value)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}
			writer.WriteStartObject();
			string __value_Code = value.Code;
			if (__value_Code != null)
			{
				writer.WriteString(QuestJsonContext.PropName_code, __value_Code);
			}
			string __value_Nbt = value.Nbt;
			if (__value_Nbt != null)
			{
				writer.WriteString(QuestJsonContext.PropName_nbt, __value_Nbt);
			}
			writer.WriteEndObject();
		}

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x0600013D RID: 317 RVA: 0x0000CEEC File Offset: 0x0000B0EC
		public JsonTypeInfo<ObjectiveProgress> ObjectiveProgress
		{
			get
			{
				JsonTypeInfo<ObjectiveProgress> result;
				if ((result = this._ObjectiveProgress) == null)
				{
					result = (this._ObjectiveProgress = (JsonTypeInfo<ObjectiveProgress>)base.Options.GetTypeInfo(typeof(ObjectiveProgress)));
				}
				return result;
			}
		}

		// Token: 0x0600013E RID: 318 RVA: 0x0000CF28 File Offset: 0x0000B128
		private JsonTypeInfo<ObjectiveProgress> Create_ObjectiveProgress(JsonSerializerOptions options)
		{
			JsonTypeInfo<ObjectiveProgress> jsonTypeInfo;
			if (!QuestJsonContext.TryGetTypeInfoForRuntimeCustomConverter<ObjectiveProgress>(options, out jsonTypeInfo))
			{
				JsonObjectInfoValues<ObjectiveProgress> jsonObjectInfoValues = new JsonObjectInfoValues<ObjectiveProgress>();
				jsonObjectInfoValues.ObjectCreator = (() => new ObjectiveProgress());
				jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = null;
				jsonObjectInfoValues.PropertyMetadataInitializer = ((JsonSerializerContext _) => QuestJsonContext.ObjectiveProgressPropInit(options));
				jsonObjectInfoValues.ConstructorParameterMetadataInitializer = null;
				jsonObjectInfoValues.SerializeHandler = new Action<Utf8JsonWriter, ObjectiveProgress>(this.ObjectiveProgressSerializeHandler);
				JsonObjectInfoValues<ObjectiveProgress> objectInfo = jsonObjectInfoValues;
				jsonTypeInfo = JsonMetadataServices.CreateObjectInfo<ObjectiveProgress>(options, objectInfo);
				jsonTypeInfo.NumberHandling = null;
			}
			jsonTypeInfo.OriginatingResolver = this;
			return jsonTypeInfo;
		}

		// Token: 0x0600013F RID: 319 RVA: 0x0000CFD4 File Offset: 0x0000B1D4
		private static JsonPropertyInfo[] ObjectiveProgressPropInit(JsonSerializerOptions options)
		{
			JsonPropertyInfo[] array = new JsonPropertyInfo[1];
			JsonPropertyInfoValues<int> jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
			jsonPropertyInfoValues.IsProperty = true;
			jsonPropertyInfoValues.IsPublic = true;
			jsonPropertyInfoValues.IsVirtual = false;
			jsonPropertyInfoValues.DeclaringType = typeof(ObjectiveProgress);
			jsonPropertyInfoValues.Converter = null;
			jsonPropertyInfoValues.Getter = ((object obj) => ((ObjectiveProgress)obj).Current);
			jsonPropertyInfoValues.Setter = delegate(object obj, int value)
			{
				((ObjectiveProgress)obj).Current = value;
			};
			jsonPropertyInfoValues.IgnoreCondition = null;
			jsonPropertyInfoValues.HasJsonInclude = false;
			jsonPropertyInfoValues.IsExtensionData = false;
			jsonPropertyInfoValues.NumberHandling = null;
			jsonPropertyInfoValues.PropertyName = "Current";
			jsonPropertyInfoValues.JsonPropertyName = "current";
			JsonPropertyInfoValues<int> info0 = jsonPropertyInfoValues;
			array[0] = JsonMetadataServices.CreatePropertyInfo<int>(options, info0);
			return array;
		}

		// Token: 0x06000140 RID: 320 RVA: 0x0000D0AF File Offset: 0x0000B2AF
		private void ObjectiveProgressSerializeHandler(Utf8JsonWriter writer, [Nullable(2)] ObjectiveProgress value)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}
			writer.WriteStartObject();
			writer.WriteNumber(QuestJsonContext.PropName_current, value.Current);
			writer.WriteEndObject();
		}

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x06000141 RID: 321 RVA: 0x0000D0D8 File Offset: 0x0000B2D8
		public JsonTypeInfo<Quest> Quest
		{
			get
			{
				JsonTypeInfo<Quest> result;
				if ((result = this._Quest) == null)
				{
					result = (this._Quest = (JsonTypeInfo<Quest>)base.Options.GetTypeInfo(typeof(Quest)));
				}
				return result;
			}
		}

		// Token: 0x06000142 RID: 322 RVA: 0x0000D114 File Offset: 0x0000B314
		private JsonTypeInfo<Quest> Create_Quest(JsonSerializerOptions options)
		{
			JsonTypeInfo<Quest> jsonTypeInfo;
			if (!QuestJsonContext.TryGetTypeInfoForRuntimeCustomConverter<Quest>(options, out jsonTypeInfo))
			{
				JsonObjectInfoValues<Quest> jsonObjectInfoValues = new JsonObjectInfoValues<Quest>();
				jsonObjectInfoValues.ObjectCreator = (() => new Quest());
				jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = null;
				jsonObjectInfoValues.PropertyMetadataInitializer = ((JsonSerializerContext _) => QuestJsonContext.QuestPropInit(options));
				jsonObjectInfoValues.ConstructorParameterMetadataInitializer = null;
				jsonObjectInfoValues.SerializeHandler = null;
				JsonObjectInfoValues<Quest> objectInfo = jsonObjectInfoValues;
				jsonTypeInfo = JsonMetadataServices.CreateObjectInfo<Quest>(options, objectInfo);
				jsonTypeInfo.NumberHandling = null;
			}
			jsonTypeInfo.OriginatingResolver = this;
			return jsonTypeInfo;
		}

		// Token: 0x06000143 RID: 323 RVA: 0x0000D1B4 File Offset: 0x0000B3B4
		private static JsonPropertyInfo[] QuestPropInit(JsonSerializerOptions options)
		{
			JsonPropertyInfo[] array = new JsonPropertyInfo[11];
			JsonPropertyInfoValues<int> jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
			jsonPropertyInfoValues.IsProperty = true;
			jsonPropertyInfoValues.IsPublic = true;
			jsonPropertyInfoValues.IsVirtual = false;
			jsonPropertyInfoValues.DeclaringType = typeof(Quest);
			jsonPropertyInfoValues.Converter = null;
			jsonPropertyInfoValues.Getter = ((object obj) => ((Quest)obj).Id);
			jsonPropertyInfoValues.Setter = delegate(object obj, int value)
			{
				((Quest)obj).Id = value;
			};
			jsonPropertyInfoValues.IgnoreCondition = null;
			jsonPropertyInfoValues.HasJsonInclude = false;
			jsonPropertyInfoValues.IsExtensionData = false;
			jsonPropertyInfoValues.NumberHandling = null;
			jsonPropertyInfoValues.PropertyName = "Id";
			jsonPropertyInfoValues.JsonPropertyName = null;
			JsonPropertyInfoValues<int> info0 = jsonPropertyInfoValues;
			array[0] = JsonMetadataServices.CreatePropertyInfo<int>(options, info0);
			JsonPropertyInfoValues<QuestRecurrenceType> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<QuestRecurrenceType>();
			jsonPropertyInfoValues2.IsProperty = true;
			jsonPropertyInfoValues2.IsPublic = true;
			jsonPropertyInfoValues2.IsVirtual = false;
			jsonPropertyInfoValues2.DeclaringType = typeof(Quest);
			jsonPropertyInfoValues2.Converter = (JsonConverter<QuestRecurrenceType>)QuestJsonContext.ExpandConverter(typeof(QuestRecurrenceType), new JsonStringEnumConverter<QuestRecurrenceType>(), options, true);
			jsonPropertyInfoValues2.Getter = ((object obj) => ((Quest)obj).RecurrenceType);
			jsonPropertyInfoValues2.Setter = delegate(object obj, QuestRecurrenceType value)
			{
				((Quest)obj).RecurrenceType = value;
			};
			jsonPropertyInfoValues2.IgnoreCondition = null;
			jsonPropertyInfoValues2.HasJsonInclude = false;
			jsonPropertyInfoValues2.IsExtensionData = false;
			jsonPropertyInfoValues2.NumberHandling = null;
			jsonPropertyInfoValues2.PropertyName = "RecurrenceType";
			jsonPropertyInfoValues2.JsonPropertyName = null;
			JsonPropertyInfoValues<QuestRecurrenceType> info = jsonPropertyInfoValues2;
			array[1] = JsonMetadataServices.CreatePropertyInfo<QuestRecurrenceType>(options, info);
			JsonPropertyInfoValues<string> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<string>();
			jsonPropertyInfoValues3.IsProperty = true;
			jsonPropertyInfoValues3.IsPublic = true;
			jsonPropertyInfoValues3.IsVirtual = false;
			jsonPropertyInfoValues3.DeclaringType = typeof(Quest);
			jsonPropertyInfoValues3.Converter = null;
			jsonPropertyInfoValues3.Getter = ((object obj) => ((Quest)obj).Title);
			jsonPropertyInfoValues3.Setter = delegate(object obj, [Nullable(2)] string value)
			{
				((Quest)obj).Title = value;
			};
			jsonPropertyInfoValues3.IgnoreCondition = null;
			jsonPropertyInfoValues3.HasJsonInclude = false;
			jsonPropertyInfoValues3.IsExtensionData = false;
			jsonPropertyInfoValues3.NumberHandling = null;
			jsonPropertyInfoValues3.PropertyName = "Title";
			jsonPropertyInfoValues3.JsonPropertyName = null;
			JsonPropertyInfoValues<string> info2 = jsonPropertyInfoValues3;
			array[2] = JsonMetadataServices.CreatePropertyInfo<string>(options, info2);
			JsonPropertyInfoValues<string> jsonPropertyInfoValues4 = new JsonPropertyInfoValues<string>();
			jsonPropertyInfoValues4.IsProperty = true;
			jsonPropertyInfoValues4.IsPublic = true;
			jsonPropertyInfoValues4.IsVirtual = false;
			jsonPropertyInfoValues4.DeclaringType = typeof(Quest);
			jsonPropertyInfoValues4.Converter = null;
			jsonPropertyInfoValues4.Getter = ((object obj) => ((Quest)obj).Description);
			jsonPropertyInfoValues4.Setter = delegate(object obj, [Nullable(2)] string value)
			{
				((Quest)obj).Description = value;
			};
			jsonPropertyInfoValues4.IgnoreCondition = null;
			jsonPropertyInfoValues4.HasJsonInclude = false;
			jsonPropertyInfoValues4.IsExtensionData = false;
			jsonPropertyInfoValues4.NumberHandling = null;
			jsonPropertyInfoValues4.PropertyName = "Description";
			jsonPropertyInfoValues4.JsonPropertyName = null;
			JsonPropertyInfoValues<string> info3 = jsonPropertyInfoValues4;
			array[3] = JsonMetadataServices.CreatePropertyInfo<string>(options, info3);
			JsonPropertyInfoValues<List<QuestObjective>> jsonPropertyInfoValues5 = new JsonPropertyInfoValues<List<QuestObjective>>();
			jsonPropertyInfoValues5.IsProperty = true;
			jsonPropertyInfoValues5.IsPublic = true;
			jsonPropertyInfoValues5.IsVirtual = false;
			jsonPropertyInfoValues5.DeclaringType = typeof(Quest);
			jsonPropertyInfoValues5.Converter = null;
			jsonPropertyInfoValues5.Getter = ((object obj) => ((Quest)obj).Objectives);
			jsonPropertyInfoValues5.Setter = delegate(object obj, [Nullable(new byte[]
			{
				2,
				1
			})] List<QuestObjective> value)
			{
				((Quest)obj).Objectives = value;
			};
			jsonPropertyInfoValues5.IgnoreCondition = null;
			jsonPropertyInfoValues5.HasJsonInclude = false;
			jsonPropertyInfoValues5.IsExtensionData = false;
			jsonPropertyInfoValues5.NumberHandling = null;
			jsonPropertyInfoValues5.PropertyName = "Objectives";
			jsonPropertyInfoValues5.JsonPropertyName = null;
			JsonPropertyInfoValues<List<QuestObjective>> info4 = jsonPropertyInfoValues5;
			array[4] = JsonMetadataServices.CreatePropertyInfo<List<QuestObjective>>(options, info4);
			JsonPropertyInfoValues<List<QuestReward>> jsonPropertyInfoValues6 = new JsonPropertyInfoValues<List<QuestReward>>();
			jsonPropertyInfoValues6.IsProperty = true;
			jsonPropertyInfoValues6.IsPublic = true;
			jsonPropertyInfoValues6.IsVirtual = false;
			jsonPropertyInfoValues6.DeclaringType = typeof(Quest);
			jsonPropertyInfoValues6.Converter = null;
			jsonPropertyInfoValues6.Getter = ((object obj) => ((Quest)obj).Rewards);
			jsonPropertyInfoValues6.Setter = delegate(object obj, [Nullable(new byte[]
			{
				2,
				1
			})] List<QuestReward> value)
			{
				((Quest)obj).Rewards = value;
			};
			jsonPropertyInfoValues6.IgnoreCondition = null;
			jsonPropertyInfoValues6.HasJsonInclude = false;
			jsonPropertyInfoValues6.IsExtensionData = false;
			jsonPropertyInfoValues6.NumberHandling = null;
			jsonPropertyInfoValues6.PropertyName = "Rewards";
			jsonPropertyInfoValues6.JsonPropertyName = null;
			JsonPropertyInfoValues<List<QuestReward>> info5 = jsonPropertyInfoValues6;
			array[5] = JsonMetadataServices.CreatePropertyInfo<List<QuestReward>>(options, info5);
			JsonPropertyInfoValues<string> jsonPropertyInfoValues7 = new JsonPropertyInfoValues<string>();
			jsonPropertyInfoValues7.IsProperty = true;
			jsonPropertyInfoValues7.IsPublic = true;
			jsonPropertyInfoValues7.IsVirtual = false;
			jsonPropertyInfoValues7.DeclaringType = typeof(Quest);
			jsonPropertyInfoValues7.Converter = null;
			jsonPropertyInfoValues7.Getter = ((object obj) => ((Quest)obj).StartsAt);
			jsonPropertyInfoValues7.Setter = delegate(object obj, [Nullable(2)] string value)
			{
				((Quest)obj).StartsAt = value;
			};
			jsonPropertyInfoValues7.IgnoreCondition = null;
			jsonPropertyInfoValues7.HasJsonInclude = false;
			jsonPropertyInfoValues7.IsExtensionData = false;
			jsonPropertyInfoValues7.NumberHandling = null;
			jsonPropertyInfoValues7.PropertyName = "StartsAt";
			jsonPropertyInfoValues7.JsonPropertyName = null;
			JsonPropertyInfoValues<string> info6 = jsonPropertyInfoValues7;
			array[6] = JsonMetadataServices.CreatePropertyInfo<string>(options, info6);
			JsonPropertyInfoValues<string> jsonPropertyInfoValues8 = new JsonPropertyInfoValues<string>();
			jsonPropertyInfoValues8.IsProperty = true;
			jsonPropertyInfoValues8.IsPublic = true;
			jsonPropertyInfoValues8.IsVirtual = false;
			jsonPropertyInfoValues8.DeclaringType = typeof(Quest);
			jsonPropertyInfoValues8.Converter = null;
			jsonPropertyInfoValues8.Getter = ((object obj) => ((Quest)obj).ExpiresAt);
			jsonPropertyInfoValues8.Setter = delegate(object obj, [Nullable(2)] string value)
			{
				((Quest)obj).ExpiresAt = value;
			};
			jsonPropertyInfoValues8.IgnoreCondition = null;
			jsonPropertyInfoValues8.HasJsonInclude = false;
			jsonPropertyInfoValues8.IsExtensionData = false;
			jsonPropertyInfoValues8.NumberHandling = null;
			jsonPropertyInfoValues8.PropertyName = "ExpiresAt";
			jsonPropertyInfoValues8.JsonPropertyName = null;
			JsonPropertyInfoValues<string> info7 = jsonPropertyInfoValues8;
			array[7] = JsonMetadataServices.CreatePropertyInfo<string>(options, info7);
			JsonPropertyInfoValues<bool> jsonPropertyInfoValues9 = new JsonPropertyInfoValues<bool>();
			jsonPropertyInfoValues9.IsProperty = true;
			jsonPropertyInfoValues9.IsPublic = true;
			jsonPropertyInfoValues9.IsVirtual = false;
			jsonPropertyInfoValues9.DeclaringType = typeof(Quest);
			jsonPropertyInfoValues9.Converter = null;
			jsonPropertyInfoValues9.Getter = ((object obj) => ((Quest)obj).UsesIngameTime);
			jsonPropertyInfoValues9.Setter = delegate(object obj, bool value)
			{
				((Quest)obj).UsesIngameTime = value;
			};
			jsonPropertyInfoValues9.IgnoreCondition = null;
			jsonPropertyInfoValues9.HasJsonInclude = false;
			jsonPropertyInfoValues9.IsExtensionData = false;
			jsonPropertyInfoValues9.NumberHandling = null;
			jsonPropertyInfoValues9.PropertyName = "UsesIngameTime";
			jsonPropertyInfoValues9.JsonPropertyName = null;
			JsonPropertyInfoValues<bool> info8 = jsonPropertyInfoValues9;
			array[8] = JsonMetadataServices.CreatePropertyInfo<bool>(options, info8);
			JsonPropertyInfoValues<bool> jsonPropertyInfoValues10 = new JsonPropertyInfoValues<bool>();
			jsonPropertyInfoValues10.IsProperty = true;
			jsonPropertyInfoValues10.IsPublic = true;
			jsonPropertyInfoValues10.IsVirtual = false;
			jsonPropertyInfoValues10.DeclaringType = typeof(Quest);
			jsonPropertyInfoValues10.Converter = null;
			jsonPropertyInfoValues10.Getter = ((object obj) => ((Quest)obj).Repeat);
			jsonPropertyInfoValues10.Setter = delegate(object obj, bool value)
			{
				((Quest)obj).Repeat = value;
			};
			jsonPropertyInfoValues10.IgnoreCondition = null;
			jsonPropertyInfoValues10.HasJsonInclude = false;
			jsonPropertyInfoValues10.IsExtensionData = false;
			jsonPropertyInfoValues10.NumberHandling = null;
			jsonPropertyInfoValues10.PropertyName = "Repeat";
			jsonPropertyInfoValues10.JsonPropertyName = null;
			JsonPropertyInfoValues<bool> info9 = jsonPropertyInfoValues10;
			array[9] = JsonMetadataServices.CreatePropertyInfo<bool>(options, info9);
			JsonPropertyInfoValues<long> jsonPropertyInfoValues11 = new JsonPropertyInfoValues<long>();
			jsonPropertyInfoValues11.IsProperty = true;
			jsonPropertyInfoValues11.IsPublic = true;
			jsonPropertyInfoValues11.IsVirtual = false;
			jsonPropertyInfoValues11.DeclaringType = typeof(Quest);
			jsonPropertyInfoValues11.Converter = null;
			jsonPropertyInfoValues11.Getter = ((object obj) => ((Quest)obj).CreatedAt);
			jsonPropertyInfoValues11.Setter = delegate(object obj, long value)
			{
				((Quest)obj).CreatedAt = value;
			};
			jsonPropertyInfoValues11.IgnoreCondition = null;
			jsonPropertyInfoValues11.HasJsonInclude = false;
			jsonPropertyInfoValues11.IsExtensionData = false;
			jsonPropertyInfoValues11.NumberHandling = null;
			jsonPropertyInfoValues11.PropertyName = "CreatedAt";
			jsonPropertyInfoValues11.JsonPropertyName = null;
			JsonPropertyInfoValues<long> info10 = jsonPropertyInfoValues11;
			array[10] = JsonMetadataServices.CreatePropertyInfo<long>(options, info10);
			return array;
		}

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x06000144 RID: 324 RVA: 0x0000DA74 File Offset: 0x0000BC74
		public JsonTypeInfo<QuestObjective> QuestObjective
		{
			get
			{
				JsonTypeInfo<QuestObjective> result;
				if ((result = this._QuestObjective) == null)
				{
					result = (this._QuestObjective = (JsonTypeInfo<QuestObjective>)base.Options.GetTypeInfo(typeof(QuestObjective)));
				}
				return result;
			}
		}

		// Token: 0x06000145 RID: 325 RVA: 0x0000DAB0 File Offset: 0x0000BCB0
		private JsonTypeInfo<QuestObjective> Create_QuestObjective(JsonSerializerOptions options)
		{
			JsonTypeInfo<QuestObjective> jsonTypeInfo;
			if (!QuestJsonContext.TryGetTypeInfoForRuntimeCustomConverter<QuestObjective>(options, out jsonTypeInfo))
			{
				JsonObjectInfoValues<QuestObjective> jsonObjectInfoValues = new JsonObjectInfoValues<QuestObjective>();
				jsonObjectInfoValues.ObjectCreator = (() => new QuestObjective());
				jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = null;
				jsonObjectInfoValues.PropertyMetadataInitializer = ((JsonSerializerContext _) => QuestJsonContext.QuestObjectivePropInit(options));
				jsonObjectInfoValues.ConstructorParameterMetadataInitializer = null;
				jsonObjectInfoValues.SerializeHandler = new Action<Utf8JsonWriter, QuestObjective>(this.QuestObjectiveSerializeHandler);
				JsonObjectInfoValues<QuestObjective> objectInfo = jsonObjectInfoValues;
				jsonTypeInfo = JsonMetadataServices.CreateObjectInfo<QuestObjective>(options, objectInfo);
				jsonTypeInfo.NumberHandling = null;
			}
			jsonTypeInfo.OriginatingResolver = this;
			return jsonTypeInfo;
		}

		// Token: 0x06000146 RID: 326 RVA: 0x0000DB5C File Offset: 0x0000BD5C
		private static JsonPropertyInfo[] QuestObjectivePropInit(JsonSerializerOptions options)
		{
			JsonPropertyInfo[] array = new JsonPropertyInfo[5];
			JsonPropertyInfoValues<int> jsonPropertyInfoValues = new JsonPropertyInfoValues<int>();
			jsonPropertyInfoValues.IsProperty = true;
			jsonPropertyInfoValues.IsPublic = true;
			jsonPropertyInfoValues.IsVirtual = false;
			jsonPropertyInfoValues.DeclaringType = typeof(QuestObjective);
			jsonPropertyInfoValues.Converter = null;
			jsonPropertyInfoValues.Getter = ((object obj) => ((QuestObjective)obj).Id);
			jsonPropertyInfoValues.Setter = delegate(object obj, int value)
			{
				((QuestObjective)obj).Id = value;
			};
			jsonPropertyInfoValues.IgnoreCondition = null;
			jsonPropertyInfoValues.HasJsonInclude = false;
			jsonPropertyInfoValues.IsExtensionData = false;
			jsonPropertyInfoValues.NumberHandling = null;
			jsonPropertyInfoValues.PropertyName = "Id";
			jsonPropertyInfoValues.JsonPropertyName = "id";
			JsonPropertyInfoValues<int> info0 = jsonPropertyInfoValues;
			array[0] = JsonMetadataServices.CreatePropertyInfo<int>(options, info0);
			JsonPropertyInfoValues<string> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<string>();
			jsonPropertyInfoValues2.IsProperty = true;
			jsonPropertyInfoValues2.IsPublic = true;
			jsonPropertyInfoValues2.IsVirtual = false;
			jsonPropertyInfoValues2.DeclaringType = typeof(QuestObjective);
			jsonPropertyInfoValues2.Converter = null;
			jsonPropertyInfoValues2.Getter = ((object obj) => ((QuestObjective)obj).Type);
			jsonPropertyInfoValues2.Setter = delegate(object obj, [Nullable(2)] string value)
			{
				((QuestObjective)obj).Type = value;
			};
			jsonPropertyInfoValues2.IgnoreCondition = null;
			jsonPropertyInfoValues2.HasJsonInclude = false;
			jsonPropertyInfoValues2.IsExtensionData = false;
			jsonPropertyInfoValues2.NumberHandling = null;
			jsonPropertyInfoValues2.PropertyName = "Type";
			jsonPropertyInfoValues2.JsonPropertyName = "type";
			JsonPropertyInfoValues<string> info = jsonPropertyInfoValues2;
			array[1] = JsonMetadataServices.CreatePropertyInfo<string>(options, info);
			JsonPropertyInfoValues<int> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<int>();
			jsonPropertyInfoValues3.IsProperty = true;
			jsonPropertyInfoValues3.IsPublic = true;
			jsonPropertyInfoValues3.IsVirtual = false;
			jsonPropertyInfoValues3.DeclaringType = typeof(QuestObjective);
			jsonPropertyInfoValues3.Converter = null;
			jsonPropertyInfoValues3.Getter = ((object obj) => ((QuestObjective)obj).Count);
			jsonPropertyInfoValues3.Setter = delegate(object obj, int value)
			{
				((QuestObjective)obj).Count = value;
			};
			jsonPropertyInfoValues3.IgnoreCondition = null;
			jsonPropertyInfoValues3.HasJsonInclude = false;
			jsonPropertyInfoValues3.IsExtensionData = false;
			jsonPropertyInfoValues3.NumberHandling = null;
			jsonPropertyInfoValues3.PropertyName = "Count";
			jsonPropertyInfoValues3.JsonPropertyName = "count";
			JsonPropertyInfoValues<int> info2 = jsonPropertyInfoValues3;
			array[2] = JsonMetadataServices.CreatePropertyInfo<int>(options, info2);
			JsonPropertyInfoValues<List<string>> jsonPropertyInfoValues4 = new JsonPropertyInfoValues<List<string>>();
			jsonPropertyInfoValues4.IsProperty = true;
			jsonPropertyInfoValues4.IsPublic = true;
			jsonPropertyInfoValues4.IsVirtual = false;
			jsonPropertyInfoValues4.DeclaringType = typeof(QuestObjective);
			jsonPropertyInfoValues4.Converter = null;
			jsonPropertyInfoValues4.Getter = ((object obj) => ((QuestObjective)obj).AcceptedTargets);
			jsonPropertyInfoValues4.Setter = delegate(object obj, [Nullable(new byte[]
			{
				2,
				1
			})] List<string> value)
			{
				((QuestObjective)obj).AcceptedTargets = value;
			};
			jsonPropertyInfoValues4.IgnoreCondition = null;
			jsonPropertyInfoValues4.HasJsonInclude = false;
			jsonPropertyInfoValues4.IsExtensionData = false;
			jsonPropertyInfoValues4.NumberHandling = null;
			jsonPropertyInfoValues4.PropertyName = "AcceptedTargets";
			jsonPropertyInfoValues4.JsonPropertyName = "acceptedTargets";
			JsonPropertyInfoValues<List<string>> info3 = jsonPropertyInfoValues4;
			array[3] = JsonMetadataServices.CreatePropertyInfo<List<string>>(options, info3);
			JsonPropertyInfoValues<List<QuestAcceptedItemDto>> jsonPropertyInfoValues5 = new JsonPropertyInfoValues<List<QuestAcceptedItemDto>>();
			jsonPropertyInfoValues5.IsProperty = true;
			jsonPropertyInfoValues5.IsPublic = true;
			jsonPropertyInfoValues5.IsVirtual = false;
			jsonPropertyInfoValues5.DeclaringType = typeof(QuestObjective);
			jsonPropertyInfoValues5.Converter = null;
			jsonPropertyInfoValues5.Getter = ((object obj) => ((QuestObjective)obj).AcceptedItems);
			jsonPropertyInfoValues5.Setter = delegate(object obj, [Nullable(new byte[]
			{
				2,
				1
			})] List<QuestAcceptedItemDto> value)
			{
				((QuestObjective)obj).AcceptedItems = value;
			};
			jsonPropertyInfoValues5.IgnoreCondition = null;
			jsonPropertyInfoValues5.HasJsonInclude = false;
			jsonPropertyInfoValues5.IsExtensionData = false;
			jsonPropertyInfoValues5.NumberHandling = null;
			jsonPropertyInfoValues5.PropertyName = "AcceptedItems";
			jsonPropertyInfoValues5.JsonPropertyName = "acceptedItems";
			JsonPropertyInfoValues<List<QuestAcceptedItemDto>> info4 = jsonPropertyInfoValues5;
			array[4] = JsonMetadataServices.CreatePropertyInfo<List<QuestAcceptedItemDto>>(options, info4);
			return array;
		}

		// Token: 0x06000147 RID: 327 RVA: 0x0000DF64 File Offset: 0x0000C164
		private void QuestObjectiveSerializeHandler(Utf8JsonWriter writer, [Nullable(2)] QuestObjective value)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}
			writer.WriteStartObject();
			writer.WriteNumber(QuestJsonContext.PropName_id, value.Id);
			string __value_Type = value.Type;
			if (__value_Type != null)
			{
				writer.WriteString(QuestJsonContext.PropName_type, __value_Type);
			}
			writer.WriteNumber(QuestJsonContext.PropName_count, value.Count);
			List<string> __value_AcceptedTargets = value.AcceptedTargets;
			if (__value_AcceptedTargets != null)
			{
				writer.WritePropertyName(QuestJsonContext.PropName_acceptedTargets);
				this.ListStringSerializeHandler(writer, __value_AcceptedTargets);
			}
			List<QuestAcceptedItemDto> __value_AcceptedItems = value.AcceptedItems;
			if (__value_AcceptedItems != null)
			{
				writer.WritePropertyName(QuestJsonContext.PropName_acceptedItems);
				this.ListQuestAcceptedItemDtoSerializeHandler(writer, __value_AcceptedItems);
			}
			writer.WriteEndObject();
		}

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x06000148 RID: 328 RVA: 0x0000DFFC File Offset: 0x0000C1FC
		public JsonTypeInfo<QuestProgressJson> QuestProgressJson
		{
			get
			{
				JsonTypeInfo<QuestProgressJson> result;
				if ((result = this._QuestProgressJson) == null)
				{
					result = (this._QuestProgressJson = (JsonTypeInfo<QuestProgressJson>)base.Options.GetTypeInfo(typeof(QuestProgressJson)));
				}
				return result;
			}
		}

		// Token: 0x06000149 RID: 329 RVA: 0x0000E038 File Offset: 0x0000C238
		private JsonTypeInfo<QuestProgressJson> Create_QuestProgressJson(JsonSerializerOptions options)
		{
			JsonTypeInfo<QuestProgressJson> jsonTypeInfo;
			if (!QuestJsonContext.TryGetTypeInfoForRuntimeCustomConverter<QuestProgressJson>(options, out jsonTypeInfo))
			{
				JsonObjectInfoValues<QuestProgressJson> jsonObjectInfoValues = new JsonObjectInfoValues<QuestProgressJson>();
				jsonObjectInfoValues.ObjectCreator = (() => new QuestProgressJson());
				jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = null;
				jsonObjectInfoValues.PropertyMetadataInitializer = ((JsonSerializerContext _) => QuestJsonContext.QuestProgressJsonPropInit(options));
				jsonObjectInfoValues.ConstructorParameterMetadataInitializer = null;
				jsonObjectInfoValues.SerializeHandler = new Action<Utf8JsonWriter, QuestProgressJson>(this.QuestProgressJsonSerializeHandler);
				JsonObjectInfoValues<QuestProgressJson> objectInfo = jsonObjectInfoValues;
				jsonTypeInfo = JsonMetadataServices.CreateObjectInfo<QuestProgressJson>(options, objectInfo);
				jsonTypeInfo.NumberHandling = null;
			}
			jsonTypeInfo.OriginatingResolver = this;
			return jsonTypeInfo;
		}

		// Token: 0x0600014A RID: 330 RVA: 0x0000E0E4 File Offset: 0x0000C2E4
		private static JsonPropertyInfo[] QuestProgressJsonPropInit(JsonSerializerOptions options)
		{
			JsonPropertyInfo[] array = new JsonPropertyInfo[1];
			JsonPropertyInfoValues<Dictionary<int, ObjectiveProgress>> jsonPropertyInfoValues = new JsonPropertyInfoValues<Dictionary<int, ObjectiveProgress>>();
			jsonPropertyInfoValues.IsProperty = true;
			jsonPropertyInfoValues.IsPublic = true;
			jsonPropertyInfoValues.IsVirtual = false;
			jsonPropertyInfoValues.DeclaringType = typeof(QuestProgressJson);
			jsonPropertyInfoValues.Converter = null;
			jsonPropertyInfoValues.Getter = ((object obj) => ((QuestProgressJson)obj).Objectives);
			jsonPropertyInfoValues.Setter = delegate(object obj, [Nullable(new byte[]
			{
				2,
				1
			})] Dictionary<int, ObjectiveProgress> value)
			{
				((QuestProgressJson)obj).Objectives = value;
			};
			jsonPropertyInfoValues.IgnoreCondition = null;
			jsonPropertyInfoValues.HasJsonInclude = false;
			jsonPropertyInfoValues.IsExtensionData = false;
			jsonPropertyInfoValues.NumberHandling = null;
			jsonPropertyInfoValues.PropertyName = "Objectives";
			jsonPropertyInfoValues.JsonPropertyName = "objectives";
			JsonPropertyInfoValues<Dictionary<int, ObjectiveProgress>> info0 = jsonPropertyInfoValues;
			array[0] = JsonMetadataServices.CreatePropertyInfo<Dictionary<int, ObjectiveProgress>>(options, info0);
			return array;
		}

		// Token: 0x0600014B RID: 331 RVA: 0x0000E1C0 File Offset: 0x0000C3C0
		private void QuestProgressJsonSerializeHandler(Utf8JsonWriter writer, [Nullable(2)] QuestProgressJson value)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}
			writer.WriteStartObject();
			Dictionary<int, ObjectiveProgress> __value_Objectives = value.Objectives;
			if (__value_Objectives != null)
			{
				writer.WritePropertyName(QuestJsonContext.PropName_objectives);
				JsonSerializer.Serialize<Dictionary<int, ObjectiveProgress>>(writer, __value_Objectives, this.DictionaryInt32ObjectiveProgress);
			}
			writer.WriteEndObject();
		}

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x0600014C RID: 332 RVA: 0x0000E208 File Offset: 0x0000C408
		public JsonTypeInfo<QuestRecurrenceType> QuestRecurrenceType
		{
			get
			{
				JsonTypeInfo<QuestRecurrenceType> result;
				if ((result = this._QuestRecurrenceType) == null)
				{
					result = (this._QuestRecurrenceType = (JsonTypeInfo<QuestRecurrenceType>)base.Options.GetTypeInfo(typeof(QuestRecurrenceType)));
				}
				return result;
			}
		}

		// Token: 0x0600014D RID: 333 RVA: 0x0000E244 File Offset: 0x0000C444
		private JsonTypeInfo<QuestRecurrenceType> Create_QuestRecurrenceType(JsonSerializerOptions options)
		{
			JsonTypeInfo<QuestRecurrenceType> jsonTypeInfo;
			if (!QuestJsonContext.TryGetTypeInfoForRuntimeCustomConverter<QuestRecurrenceType>(options, out jsonTypeInfo))
			{
				jsonTypeInfo = JsonMetadataServices.CreateValueInfo<QuestRecurrenceType>(options, JsonMetadataServices.GetEnumConverter<QuestRecurrenceType>(options));
			}
			jsonTypeInfo.OriginatingResolver = this;
			return jsonTypeInfo;
		}

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x0600014E RID: 334 RVA: 0x0000E270 File Offset: 0x0000C470
		public JsonTypeInfo<QuestRequirementsJson> QuestRequirementsJson
		{
			get
			{
				JsonTypeInfo<QuestRequirementsJson> result;
				if ((result = this._QuestRequirementsJson) == null)
				{
					result = (this._QuestRequirementsJson = (JsonTypeInfo<QuestRequirementsJson>)base.Options.GetTypeInfo(typeof(QuestRequirementsJson)));
				}
				return result;
			}
		}

		// Token: 0x0600014F RID: 335 RVA: 0x0000E2AC File Offset: 0x0000C4AC
		private JsonTypeInfo<QuestRequirementsJson> Create_QuestRequirementsJson(JsonSerializerOptions options)
		{
			JsonTypeInfo<QuestRequirementsJson> jsonTypeInfo;
			if (!QuestJsonContext.TryGetTypeInfoForRuntimeCustomConverter<QuestRequirementsJson>(options, out jsonTypeInfo))
			{
				JsonObjectInfoValues<QuestRequirementsJson> jsonObjectInfoValues = new JsonObjectInfoValues<QuestRequirementsJson>();
				jsonObjectInfoValues.ObjectCreator = (() => new QuestRequirementsJson());
				jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = null;
				jsonObjectInfoValues.PropertyMetadataInitializer = ((JsonSerializerContext _) => QuestJsonContext.QuestRequirementsJsonPropInit(options));
				jsonObjectInfoValues.ConstructorParameterMetadataInitializer = null;
				jsonObjectInfoValues.SerializeHandler = new Action<Utf8JsonWriter, QuestRequirementsJson>(this.QuestRequirementsJsonSerializeHandler);
				JsonObjectInfoValues<QuestRequirementsJson> objectInfo = jsonObjectInfoValues;
				jsonTypeInfo = JsonMetadataServices.CreateObjectInfo<QuestRequirementsJson>(options, objectInfo);
				jsonTypeInfo.NumberHandling = null;
			}
			jsonTypeInfo.OriginatingResolver = this;
			return jsonTypeInfo;
		}

		// Token: 0x06000150 RID: 336 RVA: 0x0000E358 File Offset: 0x0000C558
		private static JsonPropertyInfo[] QuestRequirementsJsonPropInit(JsonSerializerOptions options)
		{
			JsonPropertyInfo[] array = new JsonPropertyInfo[1];
			JsonPropertyInfoValues<List<QuestObjective>> jsonPropertyInfoValues = new JsonPropertyInfoValues<List<QuestObjective>>();
			jsonPropertyInfoValues.IsProperty = true;
			jsonPropertyInfoValues.IsPublic = true;
			jsonPropertyInfoValues.IsVirtual = false;
			jsonPropertyInfoValues.DeclaringType = typeof(QuestRequirementsJson);
			jsonPropertyInfoValues.Converter = null;
			jsonPropertyInfoValues.Getter = ((object obj) => ((QuestRequirementsJson)obj).Objectives);
			jsonPropertyInfoValues.Setter = delegate(object obj, [Nullable(new byte[]
			{
				2,
				1
			})] List<QuestObjective> value)
			{
				((QuestRequirementsJson)obj).Objectives = value;
			};
			jsonPropertyInfoValues.IgnoreCondition = null;
			jsonPropertyInfoValues.HasJsonInclude = false;
			jsonPropertyInfoValues.IsExtensionData = false;
			jsonPropertyInfoValues.NumberHandling = null;
			jsonPropertyInfoValues.PropertyName = "Objectives";
			jsonPropertyInfoValues.JsonPropertyName = "objectives";
			JsonPropertyInfoValues<List<QuestObjective>> info0 = jsonPropertyInfoValues;
			array[0] = JsonMetadataServices.CreatePropertyInfo<List<QuestObjective>>(options, info0);
			return array;
		}

		// Token: 0x06000151 RID: 337 RVA: 0x0000E434 File Offset: 0x0000C634
		private void QuestRequirementsJsonSerializeHandler(Utf8JsonWriter writer, [Nullable(2)] QuestRequirementsJson value)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}
			writer.WriteStartObject();
			List<QuestObjective> __value_Objectives = value.Objectives;
			if (__value_Objectives != null)
			{
				writer.WritePropertyName(QuestJsonContext.PropName_objectives);
				this.ListQuestObjectiveSerializeHandler(writer, __value_Objectives);
			}
			writer.WriteEndObject();
		}

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x06000152 RID: 338 RVA: 0x0000E474 File Offset: 0x0000C674
		public JsonTypeInfo<QuestReward> QuestReward
		{
			get
			{
				JsonTypeInfo<QuestReward> result;
				if ((result = this._QuestReward) == null)
				{
					result = (this._QuestReward = (JsonTypeInfo<QuestReward>)base.Options.GetTypeInfo(typeof(QuestReward)));
				}
				return result;
			}
		}

		// Token: 0x06000153 RID: 339 RVA: 0x0000E4B0 File Offset: 0x0000C6B0
		private JsonTypeInfo<QuestReward> Create_QuestReward(JsonSerializerOptions options)
		{
			JsonTypeInfo<QuestReward> jsonTypeInfo;
			if (!QuestJsonContext.TryGetTypeInfoForRuntimeCustomConverter<QuestReward>(options, out jsonTypeInfo))
			{
				JsonObjectInfoValues<QuestReward> jsonObjectInfoValues = new JsonObjectInfoValues<QuestReward>();
				jsonObjectInfoValues.ObjectCreator = (() => new QuestReward());
				jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = null;
				jsonObjectInfoValues.PropertyMetadataInitializer = ((JsonSerializerContext _) => QuestJsonContext.QuestRewardPropInit(options));
				jsonObjectInfoValues.ConstructorParameterMetadataInitializer = null;
				jsonObjectInfoValues.SerializeHandler = new Action<Utf8JsonWriter, QuestReward>(this.QuestRewardSerializeHandler);
				JsonObjectInfoValues<QuestReward> objectInfo = jsonObjectInfoValues;
				jsonTypeInfo = JsonMetadataServices.CreateObjectInfo<QuestReward>(options, objectInfo);
				jsonTypeInfo.NumberHandling = null;
			}
			jsonTypeInfo.OriginatingResolver = this;
			return jsonTypeInfo;
		}

		// Token: 0x06000154 RID: 340 RVA: 0x0000E55C File Offset: 0x0000C75C
		private static JsonPropertyInfo[] QuestRewardPropInit(JsonSerializerOptions options)
		{
			JsonPropertyInfo[] array = new JsonPropertyInfo[3];
			JsonPropertyInfoValues<string> jsonPropertyInfoValues = new JsonPropertyInfoValues<string>();
			jsonPropertyInfoValues.IsProperty = true;
			jsonPropertyInfoValues.IsPublic = true;
			jsonPropertyInfoValues.IsVirtual = false;
			jsonPropertyInfoValues.DeclaringType = typeof(QuestReward);
			jsonPropertyInfoValues.Converter = null;
			jsonPropertyInfoValues.Getter = ((object obj) => ((QuestReward)obj).Code);
			jsonPropertyInfoValues.Setter = delegate(object obj, [Nullable(2)] string value)
			{
				((QuestReward)obj).Code = value;
			};
			jsonPropertyInfoValues.IgnoreCondition = null;
			jsonPropertyInfoValues.HasJsonInclude = false;
			jsonPropertyInfoValues.IsExtensionData = false;
			jsonPropertyInfoValues.NumberHandling = null;
			jsonPropertyInfoValues.PropertyName = "Code";
			jsonPropertyInfoValues.JsonPropertyName = "code";
			JsonPropertyInfoValues<string> info0 = jsonPropertyInfoValues;
			array[0] = JsonMetadataServices.CreatePropertyInfo<string>(options, info0);
			JsonPropertyInfoValues<string> jsonPropertyInfoValues2 = new JsonPropertyInfoValues<string>();
			jsonPropertyInfoValues2.IsProperty = true;
			jsonPropertyInfoValues2.IsPublic = true;
			jsonPropertyInfoValues2.IsVirtual = false;
			jsonPropertyInfoValues2.DeclaringType = typeof(QuestReward);
			jsonPropertyInfoValues2.Converter = null;
			jsonPropertyInfoValues2.Getter = ((object obj) => ((QuestReward)obj).Nbt);
			jsonPropertyInfoValues2.Setter = delegate(object obj, [Nullable(2)] string value)
			{
				((QuestReward)obj).Nbt = value;
			};
			jsonPropertyInfoValues2.IgnoreCondition = null;
			jsonPropertyInfoValues2.HasJsonInclude = false;
			jsonPropertyInfoValues2.IsExtensionData = false;
			jsonPropertyInfoValues2.NumberHandling = null;
			jsonPropertyInfoValues2.PropertyName = "Nbt";
			jsonPropertyInfoValues2.JsonPropertyName = "nbt";
			JsonPropertyInfoValues<string> info = jsonPropertyInfoValues2;
			array[1] = JsonMetadataServices.CreatePropertyInfo<string>(options, info);
			JsonPropertyInfoValues<int> jsonPropertyInfoValues3 = new JsonPropertyInfoValues<int>();
			jsonPropertyInfoValues3.IsProperty = true;
			jsonPropertyInfoValues3.IsPublic = true;
			jsonPropertyInfoValues3.IsVirtual = false;
			jsonPropertyInfoValues3.DeclaringType = typeof(QuestReward);
			jsonPropertyInfoValues3.Converter = null;
			jsonPropertyInfoValues3.Getter = ((object obj) => ((QuestReward)obj).Amount);
			jsonPropertyInfoValues3.Setter = delegate(object obj, int value)
			{
				((QuestReward)obj).Amount = value;
			};
			jsonPropertyInfoValues3.IgnoreCondition = null;
			jsonPropertyInfoValues3.HasJsonInclude = false;
			jsonPropertyInfoValues3.IsExtensionData = false;
			jsonPropertyInfoValues3.NumberHandling = null;
			jsonPropertyInfoValues3.PropertyName = "Amount";
			jsonPropertyInfoValues3.JsonPropertyName = "amount";
			JsonPropertyInfoValues<int> info2 = jsonPropertyInfoValues3;
			array[2] = JsonMetadataServices.CreatePropertyInfo<int>(options, info2);
			return array;
		}

		// Token: 0x06000155 RID: 341 RVA: 0x0000E7CC File Offset: 0x0000C9CC
		private void QuestRewardSerializeHandler(Utf8JsonWriter writer, [Nullable(2)] QuestReward value)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}
			writer.WriteStartObject();
			string __value_Code = value.Code;
			if (__value_Code != null)
			{
				writer.WriteString(QuestJsonContext.PropName_code, __value_Code);
			}
			string __value_Nbt = value.Nbt;
			if (__value_Nbt != null)
			{
				writer.WriteString(QuestJsonContext.PropName_nbt, __value_Nbt);
			}
			writer.WriteNumber(QuestJsonContext.PropName_amount, value.Amount);
			writer.WriteEndObject();
		}

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x06000156 RID: 342 RVA: 0x0000E82C File Offset: 0x0000CA2C
		public JsonTypeInfo<QuestRewardsJson> QuestRewardsJson
		{
			get
			{
				JsonTypeInfo<QuestRewardsJson> result;
				if ((result = this._QuestRewardsJson) == null)
				{
					result = (this._QuestRewardsJson = (JsonTypeInfo<QuestRewardsJson>)base.Options.GetTypeInfo(typeof(QuestRewardsJson)));
				}
				return result;
			}
		}

		// Token: 0x06000157 RID: 343 RVA: 0x0000E868 File Offset: 0x0000CA68
		private JsonTypeInfo<QuestRewardsJson> Create_QuestRewardsJson(JsonSerializerOptions options)
		{
			JsonTypeInfo<QuestRewardsJson> jsonTypeInfo;
			if (!QuestJsonContext.TryGetTypeInfoForRuntimeCustomConverter<QuestRewardsJson>(options, out jsonTypeInfo))
			{
				JsonObjectInfoValues<QuestRewardsJson> jsonObjectInfoValues = new JsonObjectInfoValues<QuestRewardsJson>();
				jsonObjectInfoValues.ObjectCreator = (() => new QuestRewardsJson());
				jsonObjectInfoValues.ObjectWithParameterizedConstructorCreator = null;
				jsonObjectInfoValues.PropertyMetadataInitializer = ((JsonSerializerContext _) => QuestJsonContext.QuestRewardsJsonPropInit(options));
				jsonObjectInfoValues.ConstructorParameterMetadataInitializer = null;
				jsonObjectInfoValues.SerializeHandler = new Action<Utf8JsonWriter, QuestRewardsJson>(this.QuestRewardsJsonSerializeHandler);
				JsonObjectInfoValues<QuestRewardsJson> objectInfo = jsonObjectInfoValues;
				jsonTypeInfo = JsonMetadataServices.CreateObjectInfo<QuestRewardsJson>(options, objectInfo);
				jsonTypeInfo.NumberHandling = null;
			}
			jsonTypeInfo.OriginatingResolver = this;
			return jsonTypeInfo;
		}

		// Token: 0x06000158 RID: 344 RVA: 0x0000E914 File Offset: 0x0000CB14
		private static JsonPropertyInfo[] QuestRewardsJsonPropInit(JsonSerializerOptions options)
		{
			JsonPropertyInfo[] array = new JsonPropertyInfo[1];
			JsonPropertyInfoValues<List<QuestReward>> jsonPropertyInfoValues = new JsonPropertyInfoValues<List<QuestReward>>();
			jsonPropertyInfoValues.IsProperty = true;
			jsonPropertyInfoValues.IsPublic = true;
			jsonPropertyInfoValues.IsVirtual = false;
			jsonPropertyInfoValues.DeclaringType = typeof(QuestRewardsJson);
			jsonPropertyInfoValues.Converter = null;
			jsonPropertyInfoValues.Getter = ((object obj) => ((QuestRewardsJson)obj).Rewards);
			jsonPropertyInfoValues.Setter = delegate(object obj, [Nullable(new byte[]
			{
				2,
				1
			})] List<QuestReward> value)
			{
				((QuestRewardsJson)obj).Rewards = value;
			};
			jsonPropertyInfoValues.IgnoreCondition = null;
			jsonPropertyInfoValues.HasJsonInclude = false;
			jsonPropertyInfoValues.IsExtensionData = false;
			jsonPropertyInfoValues.NumberHandling = null;
			jsonPropertyInfoValues.PropertyName = "Rewards";
			jsonPropertyInfoValues.JsonPropertyName = "rewards";
			JsonPropertyInfoValues<List<QuestReward>> info0 = jsonPropertyInfoValues;
			array[0] = JsonMetadataServices.CreatePropertyInfo<List<QuestReward>>(options, info0);
			return array;
		}

		// Token: 0x06000159 RID: 345 RVA: 0x0000E9F0 File Offset: 0x0000CBF0
		private void QuestRewardsJsonSerializeHandler(Utf8JsonWriter writer, [Nullable(2)] QuestRewardsJson value)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}
			writer.WriteStartObject();
			List<QuestReward> __value_Rewards = value.Rewards;
			if (__value_Rewards != null)
			{
				writer.WritePropertyName(QuestJsonContext.PropName_rewards);
				this.ListQuestRewardSerializeHandler(writer, __value_Rewards);
			}
			writer.WriteEndObject();
		}

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x0600015A RID: 346 RVA: 0x0000EA30 File Offset: 0x0000CC30
		public JsonTypeInfo<Dictionary<int, ObjectiveProgress>> DictionaryInt32ObjectiveProgress
		{
			get
			{
				JsonTypeInfo<Dictionary<int, ObjectiveProgress>> result;
				if ((result = this._DictionaryInt32ObjectiveProgress) == null)
				{
					result = (this._DictionaryInt32ObjectiveProgress = (JsonTypeInfo<Dictionary<int, ObjectiveProgress>>)base.Options.GetTypeInfo(typeof(Dictionary<int, ObjectiveProgress>)));
				}
				return result;
			}
		}

		// Token: 0x0600015B RID: 347 RVA: 0x0000EA6C File Offset: 0x0000CC6C
		private JsonTypeInfo<Dictionary<int, ObjectiveProgress>> Create_DictionaryInt32ObjectiveProgress(JsonSerializerOptions options)
		{
			JsonTypeInfo<Dictionary<int, ObjectiveProgress>> jsonTypeInfo;
			if (!QuestJsonContext.TryGetTypeInfoForRuntimeCustomConverter<Dictionary<int, ObjectiveProgress>>(options, out jsonTypeInfo))
			{
				JsonCollectionInfoValues<Dictionary<int, ObjectiveProgress>> jsonCollectionInfoValues = new JsonCollectionInfoValues<Dictionary<int, ObjectiveProgress>>();
				jsonCollectionInfoValues.ObjectCreator = (() => new Dictionary<int, ObjectiveProgress>());
				jsonCollectionInfoValues.SerializeHandler = null;
				JsonCollectionInfoValues<Dictionary<int, ObjectiveProgress>> info = jsonCollectionInfoValues;
				jsonTypeInfo = JsonMetadataServices.CreateDictionaryInfo<Dictionary<int, ObjectiveProgress>, int, ObjectiveProgress>(options, info);
				jsonTypeInfo.NumberHandling = null;
			}
			jsonTypeInfo.OriginatingResolver = this;
			return jsonTypeInfo;
		}

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x0600015C RID: 348 RVA: 0x0000EAD4 File Offset: 0x0000CCD4
		public JsonTypeInfo<List<QuestAcceptedItemDto>> ListQuestAcceptedItemDto
		{
			get
			{
				JsonTypeInfo<List<QuestAcceptedItemDto>> result;
				if ((result = this._ListQuestAcceptedItemDto) == null)
				{
					result = (this._ListQuestAcceptedItemDto = (JsonTypeInfo<List<QuestAcceptedItemDto>>)base.Options.GetTypeInfo(typeof(List<QuestAcceptedItemDto>)));
				}
				return result;
			}
		}

		// Token: 0x0600015D RID: 349 RVA: 0x0000EB10 File Offset: 0x0000CD10
		private JsonTypeInfo<List<QuestAcceptedItemDto>> Create_ListQuestAcceptedItemDto(JsonSerializerOptions options)
		{
			JsonTypeInfo<List<QuestAcceptedItemDto>> jsonTypeInfo;
			if (!QuestJsonContext.TryGetTypeInfoForRuntimeCustomConverter<List<QuestAcceptedItemDto>>(options, out jsonTypeInfo))
			{
				JsonCollectionInfoValues<List<QuestAcceptedItemDto>> jsonCollectionInfoValues = new JsonCollectionInfoValues<List<QuestAcceptedItemDto>>();
				jsonCollectionInfoValues.ObjectCreator = (() => new List<QuestAcceptedItemDto>());
				jsonCollectionInfoValues.SerializeHandler = new Action<Utf8JsonWriter, List<QuestAcceptedItemDto>>(this.ListQuestAcceptedItemDtoSerializeHandler);
				JsonCollectionInfoValues<List<QuestAcceptedItemDto>> info = jsonCollectionInfoValues;
				jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<QuestAcceptedItemDto>, QuestAcceptedItemDto>(options, info);
				jsonTypeInfo.NumberHandling = null;
			}
			jsonTypeInfo.OriginatingResolver = this;
			return jsonTypeInfo;
		}

		// Token: 0x0600015E RID: 350 RVA: 0x0000EB84 File Offset: 0x0000CD84
		private void ListQuestAcceptedItemDtoSerializeHandler(Utf8JsonWriter writer, [Nullable(new byte[]
		{
			2,
			1
		})] List<QuestAcceptedItemDto> value)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}
			writer.WriteStartArray();
			for (int i = 0; i < value.Count; i++)
			{
				this.QuestAcceptedItemDtoSerializeHandler(writer, value[i]);
			}
			writer.WriteEndArray();
		}

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x0600015F RID: 351 RVA: 0x0000EBC8 File Offset: 0x0000CDC8
		public JsonTypeInfo<List<QuestObjective>> ListQuestObjective
		{
			get
			{
				JsonTypeInfo<List<QuestObjective>> result;
				if ((result = this._ListQuestObjective) == null)
				{
					result = (this._ListQuestObjective = (JsonTypeInfo<List<QuestObjective>>)base.Options.GetTypeInfo(typeof(List<QuestObjective>)));
				}
				return result;
			}
		}

		// Token: 0x06000160 RID: 352 RVA: 0x0000EC04 File Offset: 0x0000CE04
		private JsonTypeInfo<List<QuestObjective>> Create_ListQuestObjective(JsonSerializerOptions options)
		{
			JsonTypeInfo<List<QuestObjective>> jsonTypeInfo;
			if (!QuestJsonContext.TryGetTypeInfoForRuntimeCustomConverter<List<QuestObjective>>(options, out jsonTypeInfo))
			{
				JsonCollectionInfoValues<List<QuestObjective>> jsonCollectionInfoValues = new JsonCollectionInfoValues<List<QuestObjective>>();
				jsonCollectionInfoValues.ObjectCreator = (() => new List<QuestObjective>());
				jsonCollectionInfoValues.SerializeHandler = new Action<Utf8JsonWriter, List<QuestObjective>>(this.ListQuestObjectiveSerializeHandler);
				JsonCollectionInfoValues<List<QuestObjective>> info = jsonCollectionInfoValues;
				jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<QuestObjective>, QuestObjective>(options, info);
				jsonTypeInfo.NumberHandling = null;
			}
			jsonTypeInfo.OriginatingResolver = this;
			return jsonTypeInfo;
		}

		// Token: 0x06000161 RID: 353 RVA: 0x0000EC78 File Offset: 0x0000CE78
		private void ListQuestObjectiveSerializeHandler(Utf8JsonWriter writer, [Nullable(new byte[]
		{
			2,
			1
		})] List<QuestObjective> value)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}
			writer.WriteStartArray();
			for (int i = 0; i < value.Count; i++)
			{
				this.QuestObjectiveSerializeHandler(writer, value[i]);
			}
			writer.WriteEndArray();
		}

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x06000162 RID: 354 RVA: 0x0000ECBC File Offset: 0x0000CEBC
		public JsonTypeInfo<List<QuestReward>> ListQuestReward
		{
			get
			{
				JsonTypeInfo<List<QuestReward>> result;
				if ((result = this._ListQuestReward) == null)
				{
					result = (this._ListQuestReward = (JsonTypeInfo<List<QuestReward>>)base.Options.GetTypeInfo(typeof(List<QuestReward>)));
				}
				return result;
			}
		}

		// Token: 0x06000163 RID: 355 RVA: 0x0000ECF8 File Offset: 0x0000CEF8
		private JsonTypeInfo<List<QuestReward>> Create_ListQuestReward(JsonSerializerOptions options)
		{
			JsonTypeInfo<List<QuestReward>> jsonTypeInfo;
			if (!QuestJsonContext.TryGetTypeInfoForRuntimeCustomConverter<List<QuestReward>>(options, out jsonTypeInfo))
			{
				JsonCollectionInfoValues<List<QuestReward>> jsonCollectionInfoValues = new JsonCollectionInfoValues<List<QuestReward>>();
				jsonCollectionInfoValues.ObjectCreator = (() => new List<QuestReward>());
				jsonCollectionInfoValues.SerializeHandler = new Action<Utf8JsonWriter, List<QuestReward>>(this.ListQuestRewardSerializeHandler);
				JsonCollectionInfoValues<List<QuestReward>> info = jsonCollectionInfoValues;
				jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<QuestReward>, QuestReward>(options, info);
				jsonTypeInfo.NumberHandling = null;
			}
			jsonTypeInfo.OriginatingResolver = this;
			return jsonTypeInfo;
		}

		// Token: 0x06000164 RID: 356 RVA: 0x0000ED6C File Offset: 0x0000CF6C
		private void ListQuestRewardSerializeHandler(Utf8JsonWriter writer, [Nullable(new byte[]
		{
			2,
			1
		})] List<QuestReward> value)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}
			writer.WriteStartArray();
			for (int i = 0; i < value.Count; i++)
			{
				this.QuestRewardSerializeHandler(writer, value[i]);
			}
			writer.WriteEndArray();
		}

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x06000165 RID: 357 RVA: 0x0000EDB0 File Offset: 0x0000CFB0
		public JsonTypeInfo<List<string>> ListString
		{
			get
			{
				JsonTypeInfo<List<string>> result;
				if ((result = this._ListString) == null)
				{
					result = (this._ListString = (JsonTypeInfo<List<string>>)base.Options.GetTypeInfo(typeof(List<string>)));
				}
				return result;
			}
		}

		// Token: 0x06000166 RID: 358 RVA: 0x0000EDEC File Offset: 0x0000CFEC
		private JsonTypeInfo<List<string>> Create_ListString(JsonSerializerOptions options)
		{
			JsonTypeInfo<List<string>> jsonTypeInfo;
			if (!QuestJsonContext.TryGetTypeInfoForRuntimeCustomConverter<List<string>>(options, out jsonTypeInfo))
			{
				JsonCollectionInfoValues<List<string>> jsonCollectionInfoValues = new JsonCollectionInfoValues<List<string>>();
				jsonCollectionInfoValues.ObjectCreator = (() => new List<string>());
				jsonCollectionInfoValues.SerializeHandler = new Action<Utf8JsonWriter, List<string>>(this.ListStringSerializeHandler);
				JsonCollectionInfoValues<List<string>> info = jsonCollectionInfoValues;
				jsonTypeInfo = JsonMetadataServices.CreateListInfo<List<string>, string>(options, info);
				jsonTypeInfo.NumberHandling = null;
			}
			jsonTypeInfo.OriginatingResolver = this;
			return jsonTypeInfo;
		}

		// Token: 0x06000167 RID: 359 RVA: 0x0000EE60 File Offset: 0x0000D060
		private void ListStringSerializeHandler(Utf8JsonWriter writer, [Nullable(new byte[]
		{
			2,
			1
		})] List<string> value)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}
			writer.WriteStartArray();
			for (int i = 0; i < value.Count; i++)
			{
				writer.WriteStringValue(value[i]);
			}
			writer.WriteEndArray();
		}

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x06000168 RID: 360 RVA: 0x0000EEA4 File Offset: 0x0000D0A4
		public JsonTypeInfo<int> Int32
		{
			get
			{
				JsonTypeInfo<int> result;
				if ((result = this._Int32) == null)
				{
					result = (this._Int32 = (JsonTypeInfo<int>)base.Options.GetTypeInfo(typeof(int)));
				}
				return result;
			}
		}

		// Token: 0x06000169 RID: 361 RVA: 0x0000EEE0 File Offset: 0x0000D0E0
		private JsonTypeInfo<int> Create_Int32(JsonSerializerOptions options)
		{
			JsonTypeInfo<int> jsonTypeInfo;
			if (!QuestJsonContext.TryGetTypeInfoForRuntimeCustomConverter<int>(options, out jsonTypeInfo))
			{
				jsonTypeInfo = JsonMetadataServices.CreateValueInfo<int>(options, JsonMetadataServices.Int32Converter);
			}
			jsonTypeInfo.OriginatingResolver = this;
			return jsonTypeInfo;
		}

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x0600016A RID: 362 RVA: 0x0000EF0C File Offset: 0x0000D10C
		public JsonTypeInfo<long> Int64
		{
			get
			{
				JsonTypeInfo<long> result;
				if ((result = this._Int64) == null)
				{
					result = (this._Int64 = (JsonTypeInfo<long>)base.Options.GetTypeInfo(typeof(long)));
				}
				return result;
			}
		}

		// Token: 0x0600016B RID: 363 RVA: 0x0000EF48 File Offset: 0x0000D148
		private JsonTypeInfo<long> Create_Int64(JsonSerializerOptions options)
		{
			JsonTypeInfo<long> jsonTypeInfo;
			if (!QuestJsonContext.TryGetTypeInfoForRuntimeCustomConverter<long>(options, out jsonTypeInfo))
			{
				jsonTypeInfo = JsonMetadataServices.CreateValueInfo<long>(options, JsonMetadataServices.Int64Converter);
			}
			jsonTypeInfo.OriginatingResolver = this;
			return jsonTypeInfo;
		}

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x0600016C RID: 364 RVA: 0x0000EF74 File Offset: 0x0000D174
		public JsonTypeInfo<string> String
		{
			get
			{
				JsonTypeInfo<string> result;
				if ((result = this._String) == null)
				{
					result = (this._String = (JsonTypeInfo<string>)base.Options.GetTypeInfo(typeof(string)));
				}
				return result;
			}
		}

		// Token: 0x0600016D RID: 365 RVA: 0x0000EFB0 File Offset: 0x0000D1B0
		private JsonTypeInfo<string> Create_String(JsonSerializerOptions options)
		{
			JsonTypeInfo<string> jsonTypeInfo;
			if (!QuestJsonContext.TryGetTypeInfoForRuntimeCustomConverter<string>(options, out jsonTypeInfo))
			{
				jsonTypeInfo = JsonMetadataServices.CreateValueInfo<string>(options, JsonMetadataServices.StringConverter);
			}
			jsonTypeInfo.OriginatingResolver = this;
			return jsonTypeInfo;
		}

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x0600016E RID: 366 RVA: 0x0000EFDB File Offset: 0x0000D1DB
		public static QuestJsonContext Default { get; } = new QuestJsonContext(new JsonSerializerOptions(QuestJsonContext.s_defaultOptions));

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x0600016F RID: 367 RVA: 0x0000EFE2 File Offset: 0x0000D1E2
		[Nullable(2)]
		protected override JsonSerializerOptions GeneratedSerializerOptions { [NullableContext(2)] get; } = QuestJsonContext.s_defaultOptions;

		// Token: 0x06000170 RID: 368 RVA: 0x0000EFEA File Offset: 0x0000D1EA
		public QuestJsonContext() : base(null)
		{
		}

		// Token: 0x06000171 RID: 369 RVA: 0x0000EFFE File Offset: 0x0000D1FE
		public QuestJsonContext(JsonSerializerOptions options) : base(options)
		{
		}

		// Token: 0x06000172 RID: 370 RVA: 0x0000F014 File Offset: 0x0000D214
		private static bool TryGetTypeInfoForRuntimeCustomConverter<[Nullable(2)] TJsonMetadataType>(JsonSerializerOptions options, out JsonTypeInfo<TJsonMetadataType> jsonTypeInfo)
		{
			JsonConverter converter = QuestJsonContext.GetRuntimeConverterForType(typeof(!!0), options);
			if (converter != null)
			{
				jsonTypeInfo = JsonMetadataServices.CreateValueInfo<TJsonMetadataType>(options, converter);
				return true;
			}
			jsonTypeInfo = null;
			return false;
		}

		// Token: 0x06000173 RID: 371 RVA: 0x0000F044 File Offset: 0x0000D244
		[return: Nullable(2)]
		private static JsonConverter GetRuntimeConverterForType(Type type, JsonSerializerOptions options)
		{
			for (int i = 0; i < options.Converters.Count; i++)
			{
				JsonConverter converter = options.Converters[i];
				if (converter != null && converter.CanConvert(type))
				{
					return QuestJsonContext.ExpandConverter(type, converter, options, false);
				}
			}
			return null;
		}

		// Token: 0x06000174 RID: 372 RVA: 0x0000F08C File Offset: 0x0000D28C
		private static JsonConverter ExpandConverter(Type type, JsonConverter converter, JsonSerializerOptions options, bool validateCanConvert = true)
		{
			if (validateCanConvert && !converter.CanConvert(type))
			{
				throw new InvalidOperationException(string.Format("The converter '{0}' is not compatible with the type '{1}'.", converter.GetType(), type));
			}
			JsonConverterFactory factory = converter as JsonConverterFactory;
			if (factory != null)
			{
				converter = factory.CreateConverter(type, options);
				if (converter == null || converter is JsonConverterFactory)
				{
					throw new InvalidOperationException(string.Format("The converter '{0}' cannot return null or a JsonConverterFactory instance.", factory.GetType()));
				}
			}
			return converter;
		}

		// Token: 0x06000175 RID: 373 RVA: 0x0000F0F4 File Offset: 0x0000D2F4
		[return: Nullable(2)]
		public override JsonTypeInfo GetTypeInfo(Type type)
		{
			JsonTypeInfo typeInfo;
			base.Options.TryGetTypeInfo(type, out typeInfo);
			return typeInfo;
		}

		// Token: 0x06000176 RID: 374 RVA: 0x0000F114 File Offset: 0x0000D314
		[return: Nullable(2)]
		JsonTypeInfo IJsonTypeInfoResolver.GetTypeInfo(Type type, JsonSerializerOptions options)
		{
			if (type == typeof(bool))
			{
				return this.Create_Boolean(options);
			}
			if (type == typeof(QuestAcceptedItemDto))
			{
				return this.Create_QuestAcceptedItemDto(options);
			}
			if (type == typeof(ObjectiveProgress))
			{
				return this.Create_ObjectiveProgress(options);
			}
			if (type == typeof(Quest))
			{
				return this.Create_Quest(options);
			}
			if (type == typeof(QuestObjective))
			{
				return this.Create_QuestObjective(options);
			}
			if (type == typeof(QuestProgressJson))
			{
				return this.Create_QuestProgressJson(options);
			}
			if (type == typeof(QuestRecurrenceType))
			{
				return this.Create_QuestRecurrenceType(options);
			}
			if (type == typeof(QuestRequirementsJson))
			{
				return this.Create_QuestRequirementsJson(options);
			}
			if (type == typeof(QuestReward))
			{
				return this.Create_QuestReward(options);
			}
			if (type == typeof(QuestRewardsJson))
			{
				return this.Create_QuestRewardsJson(options);
			}
			if (type == typeof(Dictionary<int, ObjectiveProgress>))
			{
				return this.Create_DictionaryInt32ObjectiveProgress(options);
			}
			if (type == typeof(List<QuestAcceptedItemDto>))
			{
				return this.Create_ListQuestAcceptedItemDto(options);
			}
			if (type == typeof(List<QuestObjective>))
			{
				return this.Create_ListQuestObjective(options);
			}
			if (type == typeof(List<QuestReward>))
			{
				return this.Create_ListQuestReward(options);
			}
			if (type == typeof(List<string>))
			{
				return this.Create_ListString(options);
			}
			if (type == typeof(int))
			{
				return this.Create_Int32(options);
			}
			if (type == typeof(long))
			{
				return this.Create_Int64(options);
			}
			if (type == typeof(string))
			{
				return this.Create_String(options);
			}
			return null;
		}

		// Token: 0x04000075 RID: 117
		[Nullable(2)]
		private JsonTypeInfo<bool> _Boolean;

		// Token: 0x04000076 RID: 118
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private JsonTypeInfo<QuestAcceptedItemDto> _QuestAcceptedItemDto;

		// Token: 0x04000077 RID: 119
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private JsonTypeInfo<ObjectiveProgress> _ObjectiveProgress;

		// Token: 0x04000078 RID: 120
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private JsonTypeInfo<Quest> _Quest;

		// Token: 0x04000079 RID: 121
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private JsonTypeInfo<QuestObjective> _QuestObjective;

		// Token: 0x0400007A RID: 122
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private JsonTypeInfo<QuestProgressJson> _QuestProgressJson;

		// Token: 0x0400007B RID: 123
		[Nullable(2)]
		private JsonTypeInfo<QuestRecurrenceType> _QuestRecurrenceType;

		// Token: 0x0400007C RID: 124
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private JsonTypeInfo<QuestRequirementsJson> _QuestRequirementsJson;

		// Token: 0x0400007D RID: 125
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private JsonTypeInfo<QuestReward> _QuestReward;

		// Token: 0x0400007E RID: 126
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private JsonTypeInfo<QuestRewardsJson> _QuestRewardsJson;

		// Token: 0x0400007F RID: 127
		[Nullable(new byte[]
		{
			2,
			1,
			1
		})]
		private JsonTypeInfo<Dictionary<int, ObjectiveProgress>> _DictionaryInt32ObjectiveProgress;

		// Token: 0x04000080 RID: 128
		[Nullable(new byte[]
		{
			2,
			1,
			1
		})]
		private JsonTypeInfo<List<QuestAcceptedItemDto>> _ListQuestAcceptedItemDto;

		// Token: 0x04000081 RID: 129
		[Nullable(new byte[]
		{
			2,
			1,
			1
		})]
		private JsonTypeInfo<List<QuestObjective>> _ListQuestObjective;

		// Token: 0x04000082 RID: 130
		[Nullable(new byte[]
		{
			2,
			1,
			1
		})]
		private JsonTypeInfo<List<QuestReward>> _ListQuestReward;

		// Token: 0x04000083 RID: 131
		[Nullable(new byte[]
		{
			2,
			1,
			1
		})]
		private JsonTypeInfo<List<string>> _ListString;

		// Token: 0x04000084 RID: 132
		[Nullable(2)]
		private JsonTypeInfo<int> _Int32;

		// Token: 0x04000085 RID: 133
		[Nullable(2)]
		private JsonTypeInfo<long> _Int64;

		// Token: 0x04000086 RID: 134
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private JsonTypeInfo<string> _String;

		// Token: 0x04000087 RID: 135
		private static readonly JsonSerializerOptions s_defaultOptions = new JsonSerializerOptions
		{
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		// Token: 0x0400008A RID: 138
		private static readonly JsonEncodedText PropName_code = JsonEncodedText.Encode("code", null);

		// Token: 0x0400008B RID: 139
		private static readonly JsonEncodedText PropName_nbt = JsonEncodedText.Encode("nbt", null);

		// Token: 0x0400008C RID: 140
		private static readonly JsonEncodedText PropName_current = JsonEncodedText.Encode("current", null);

		// Token: 0x0400008D RID: 141
		private static readonly JsonEncodedText PropName_id = JsonEncodedText.Encode("id", null);

		// Token: 0x0400008E RID: 142
		private static readonly JsonEncodedText PropName_type = JsonEncodedText.Encode("type", null);

		// Token: 0x0400008F RID: 143
		private static readonly JsonEncodedText PropName_count = JsonEncodedText.Encode("count", null);

		// Token: 0x04000090 RID: 144
		private static readonly JsonEncodedText PropName_acceptedTargets = JsonEncodedText.Encode("acceptedTargets", null);

		// Token: 0x04000091 RID: 145
		private static readonly JsonEncodedText PropName_acceptedItems = JsonEncodedText.Encode("acceptedItems", null);

		// Token: 0x04000092 RID: 146
		private static readonly JsonEncodedText PropName_objectives = JsonEncodedText.Encode("objectives", null);

		// Token: 0x04000093 RID: 147
		private static readonly JsonEncodedText PropName_amount = JsonEncodedText.Encode("amount", null);

		// Token: 0x04000094 RID: 148
		private static readonly JsonEncodedText PropName_rewards = JsonEncodedText.Encode("rewards", null);
	}
}
