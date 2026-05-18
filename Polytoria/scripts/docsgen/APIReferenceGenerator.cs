// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;
using Polytoria.Datamodel;
using Polytoria.Datamodel.Services;
using Polytoria.Scripting;
using Polytoria.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Polytoria.DocsGen.APIReferenceGenerator;

namespace Polytoria.DocsGen;

public class APIReferenceGenerator
{
	public static APIReferenceRoot GenerateReferences()
	{
		Assembly assembly = Assembly.GetExecutingAssembly();
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
		Type[] types = assembly.GetTypes();
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

		APIReferenceRoot apiRef = new() { Version = Globals.AppVersion, Classes = [], InstanceClasses = [] };
		List<ScriptEnum> enums = [];
		List<Type> missingEnums = [];
		Dictionary<Type, ScriptClass> classMap = [];

		XmlDocReader? xmlDocs = null;
		if (FileAccess.FileExists("res://Polytoria.xml"))
		{
			using var docFile = FileAccess.Open("res://Polytoria.xml", FileAccess.ModeFlags.Read);
			xmlDocs = new XmlDocReader(docFile.GetAsText());
		}

		foreach (Type type in types)
		{
			if (!type.IsAssignableTo(typeof(IScriptObject))) continue;
			if (type.IsEnum || type.IsInterface) continue;
			if (type.IsDefined(typeof(InternalAttribute))) continue;
			if (type.FullName == null) continue;
			if (type.FullName.Contains("Polytoria.Scripting.Extensions")) continue;
			if (type.FullName.Contains("Polytoria.Scripting.Libraries")) continue;
			if (type.IsGenericType) continue;

			if (type.IsAssignableTo(typeof(Instance)))
			{
				apiRef.InstanceClasses.Add(ProcessClassName(type));
			}

#pragma warning disable IL2075 // Datamodel types has the reflections needed
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
			MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
#pragma warning restore IL2075

			List<ScriptProperty> propertiesDef = [];
			List<ScriptMethod> methodsDef = [];
			List<ScriptEvent> eventsDef = [];

			foreach (PropertyInfo property in properties)
			{
				ScriptPropertyAttribute? propAttribute = property.GetCustomAttribute<ScriptPropertyAttribute>();
				EditableAttribute? editableAttribute = property.GetCustomAttribute<EditableAttribute>();

				if (propAttribute == null && editableAttribute == null) continue;

				if (property.PropertyType == typeof(PTSignal) ||
					(property.PropertyType.IsGenericType &&
					 property.PropertyType.GetGenericTypeDefinition().Name.StartsWith(nameof(PTSignal))))
				{
					string eventKey = "P:" + type.FullName + "." + property.Name;
					Dictionary<string, string> eventParamDocs = xmlDocs?.GetMemberParams(eventKey) ?? [];

					ScriptEvent eventDef = new()
					{
						Name = property.Name,
						Description = xmlDocs?.GetMemberSummary(eventKey),
					};

					Type propertyType = property.PropertyType;
					if (propertyType.IsGenericType)
					{
						Type[] genericArgs = propertyType.GetGenericArguments();
						List<ScriptParameter> paramsDef = [];

						for (int i = 0; i < genericArgs.Length; i++)
						{
							string tn = ProcessTypeName(genericArgs[i]) ?? "any";
							string pName = tn.ToCamelCase();
							ScriptParameter param = new()
							{
								Name = pName,
								Type = tn,
								Description = eventParamDocs.TryGetValue(pName, out string? d) ? d : null,
								IsOptional = false,
								DefaultValue = null
							};
							paramsDef.Add(param);
						}

						eventDef.Parameters = paramsDef;
					}
					else if (eventParamDocs.Count > 0)
					{
						// Non-generic PTSignal — adopt hand-documented params verbatim
						List<ScriptParameter> paramsDef = [];
						foreach ((string pName, string pDesc) in eventParamDocs)
						{
							paramsDef.Add(new ScriptParameter
							{
								Name = pName,
								Type = "any",
								Description = pDesc,
								IsOptional = false,
								DefaultValue = null
							});
						}
						eventDef.Parameters = paramsDef;
					}

					eventsDef.Add(eventDef);
				}
				else
				{
					string propKey = "P:" + type.FullName + "." + property.Name;
					ScriptProperty propDef = new()
					{
						Name = property.Name,
						Type = ProcessTypeName(property.PropertyType),
						Description = xmlDocs?.GetMemberSummary(propKey),
						Remarks = xmlDocs?.GetMemberSection(propKey, "remarks"),
						SeeAlso = xmlDocs?.GetMemberSeeAlso(propKey),
						IsAccessibleByScripts = !(editableAttribute != null && propAttribute == null),
						IsObsolete = property.GetCustomAttribute<Attributes.ObsoleteAttribute>() != null,
						IsStatic = property.GetAccessors(true)[0].IsStatic
					};

					if (propAttribute != null)
					{
						MethodInfo? setMethod = property.GetSetMethod(false);
						propDef.IsReadOnly = setMethod == null;
					}

					if (property.PropertyType.IsEnum)
					{
						if (!ScriptService.EnumMap.ContainsValue(property.PropertyType))
						{
							missingEnums.Add(property.PropertyType);
						}
					}

					propertiesDef.Add(propDef);
				}
			}

			foreach (MethodInfo method in methods)
			{
				ScriptMethodAttribute? methodAttribute = method.GetCustomAttribute<ScriptMethodAttribute>();
				ScriptMetamethodAttribute? metaMethodAttribute = method.GetCustomAttribute<ScriptMetamethodAttribute>();

				if (methodAttribute == null && metaMethodAttribute == null) continue;
				if (method.IsDefined(typeof(HandlesLuaStateAttribute))) continue;

				bool asyncFunc = false;
				Type returnType = method.ReturnType;

				if (returnType == typeof(Task))
				{
					asyncFunc = true;
				}
				else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
				{
					asyncFunc = true;
					returnType = returnType.GetGenericArguments()[0];
				}

				List<ScriptParameter> paramsDef = [];

				foreach (ParameterInfo item in method.GetParameters())
				{
					if (item.ParameterType == typeof(Node)) continue;
					if (item.IsDefined(typeof(ScriptingCallerAttribute))) continue;
					ScriptParameter param = new()
					{
						Name = item.Name ?? "",
						Type = ProcessTypeName(item.ParameterType),
						IsOptional = item.HasDefaultValue,
						DefaultValue = item.DefaultValue?.ToString() ?? null
					};

					paramsDef.Add(param);
				}

				if (returnType == typeof(Node)) continue;

				string methodName = metaMethodAttribute != null ? GetMetamethodIndexer(metaMethodAttribute.Metamethod) : methodAttribute?.MethodName ?? method.Name;
				(string? mDesc, string? mRemarks, string? mReturns, List<string>? mExamples, List<string>? mSeeAlso, Dictionary<string, string> mParamDocs) = xmlDocs?.GetMethodDocs(type.FullName!, method.Name) ?? (null, null, null, null, null, []);

				// Populate parameter descriptions from XML docs
				for (int i = 0; i < paramsDef.Count; i++)
				{
					ScriptParameter p = paramsDef[i];
					if (mParamDocs.TryGetValue(p.Name, out string? pDesc))
					{
						p.Description = pDesc;
						paramsDef[i] = p;
					}
				}

				ScriptMethod methodDef = new()
				{
					Name = methodName,
					ReturnType = ProcessTypeName(returnType),
					Returns = mReturns,
					Description = mDesc,
					Remarks = mRemarks,
					Examples = mExamples,
					SeeAlso = mSeeAlso,
					IsAsync = asyncFunc,
					Parameters = paramsDef,
					IsObsolete = method.GetCustomAttribute<Attributes.ObsoleteAttribute>() != null,
					IsStatic = method.IsStatic,
					IsSemiStatic = method.IsStatic && (methodAttribute?.SemiStatic ?? false),
				};

				methodsDef.Add(methodDef);
			}

			// __index & __newindex for Instance
			if (type == typeof(Instance))
			{
				methodsDef.Add(new()
				{
					Name = "__index",
					ReturnType = "any",
					IsAsync = false,
					Parameters =
					[
						new() { Name = "indexer", Type = "any" }
					],
					IsObsolete = false,
					IsStatic = false,
				});
				methodsDef.Add(new()
				{
					Name = "__newindex",
					ReturnType = "nil",
					IsAsync = false,
					Parameters =
					[
						new() { Name = "indexer", Type = "any" }
					],
					IsObsolete = false,
					IsStatic = false,
				});
			}

			StaticAttribute? staticA = type.GetCustomAttribute<StaticAttribute>();
			DocCategoryAttribute? categoryA = type.GetCustomAttribute<DocCategoryAttribute>();

			string typeKey = "T:" + type.FullName;
			ScriptClass typeDef = new()
			{
				Name = ProcessClassName(type),
				BaseType = ((type.BaseType != null && type.BaseType.IsAssignableTo(typeof(Node))) || type.BaseType == typeof(object) || type.BaseType == typeof(ValueType)) ? null : type.BaseType?.Name ?? null,
				Category = categoryA?.Category,
				Description = xmlDocs?.GetMemberSummary(typeKey),
				Remarks = xmlDocs?.GetMemberSection(typeKey, "remarks"),
				Examples = xmlDocs?.GetMemberExamples(typeKey),
				SeeAlso = xmlDocs?.GetMemberSeeAlso(typeKey),
				IsStatic = staticA != null,
				StaticAlias = staticA?.Alias,
				IsAbstract = type.IsDefined(typeof(AbstractAttribute), false),
				IsInstantiable = type.IsDefined(typeof(InstantiableAttribute), false),
				Properties = propertiesDef,
				Methods = methodsDef,
				Events = eventsDef,
			};

			classMap[type] = typeDef;
		}

		// Order classes by inheritance hierarchy
		List<ScriptClass> orderedClasses = OrderClassesByInheritance(classMap);
		apiRef.Classes = orderedClasses;

		foreach ((string key, Type enumType) in ScriptService.EnumMap)
		{
			List<ScriptEnumValue> options = Enum.GetNames(enumType)
				.Select(n => new ScriptEnumValue
				{
					Name = n,
					Description = xmlDocs?.GetMemberSummary("F:" + enumType.FullName + "." + n),
				})
				.ToList();
			enums.Add(new()
			{
				Name = key,
				InternalName = enumType.Name,
				Description = xmlDocs?.GetMemberSummary("T:" + enumType.FullName),
				Options = options,
			});
		}
		apiRef.Enums = enums;

		if (Globals.IsInGDEditor)
		{
			// Display enum map missing warnings
			PT.Print("APIREF Generation Complete");
			PT.Print("Missing enums: ", missingEnums.Count);
			foreach (Type item in missingEnums)
			{
				PT.PrintErr("Enum Missing ", item.Name);
			}
		}

		return apiRef;
	}

	private static string GetMetamethodIndexer(ScriptObjectMetamethod metamethod)
	{
		return metamethod switch
		{
			ScriptObjectMetamethod.Add => "__add",
			ScriptObjectMetamethod.Sub => "__sub",
			ScriptObjectMetamethod.Call => "__call",
			ScriptObjectMetamethod.Concat => "__concat",
			ScriptObjectMetamethod.Div => "__div",
			ScriptObjectMetamethod.Eq => "__eq",
			ScriptObjectMetamethod.Iter => "__iter",
			ScriptObjectMetamethod.Le => "__le",
			ScriptObjectMetamethod.Len => "__len",
			ScriptObjectMetamethod.Lt => "__lt",
			ScriptObjectMetamethod.Mod => "__mod",
			ScriptObjectMetamethod.Mul => "__mul",
			ScriptObjectMetamethod.Pow => "__pow",
			ScriptObjectMetamethod.ToString => "__tostring",
			ScriptObjectMetamethod.Unm => "__unm",
			ScriptObjectMetamethod.Index => "__index",
			ScriptObjectMetamethod.NewIndex => "__newindex",
			_ => ""
		};
	}

	private static List<ScriptClass> OrderClassesByInheritance(Dictionary<Type, ScriptClass> classMap)
	{
		List<ScriptClass> result = [];
		HashSet<Type> processed = [];
		Dictionary<Type, List<Type>> children = [];

		// Build parent-child relationships
		foreach (Type type in classMap.Keys)
		{
			Type? baseType = type.BaseType;

			// Find the actual base type
			while (baseType != null &&
				   baseType != typeof(object) &&
				   baseType != typeof(ValueType) &&
				   !baseType.IsAssignableTo(typeof(Node)))
			{
				if (classMap.ContainsKey(baseType))
				{
					if (!children.TryGetValue(baseType, out List<Type>? value))
					{
						value = [];
						children[baseType] = value;
					}

					value.Add(type);
					break;
				}
				baseType = baseType.BaseType;
			}
		}

		// Recursive function to add type and its children in order
		void AddTypeAndChildren(Type type)
		{
			if (processed.Contains(type)) return;
			if (!classMap.TryGetValue(type, out ScriptClass v)) return;

			// Ensure parent is added
			Type? baseType = type.BaseType;
			while (baseType != null &&
				   baseType != typeof(object) &&
				   baseType != typeof(ValueType) &&
				   !baseType.IsAssignableTo(typeof(Node)))
			{
				if (classMap.ContainsKey(baseType) && !processed.Contains(baseType))
				{
					AddTypeAndChildren(baseType);
					break;
				}
				baseType = baseType.BaseType;
			}

			processed.Add(type);
			result.Add(v);

			// Add children
			if (children.TryGetValue(type, out List<Type>? value))
			{
				foreach (Type child in value)
				{
					AddTypeAndChildren(child);
				}
			}
		}

		// Find root types
		List<Type> roots = [];
		foreach (Type type in classMap.Keys)
		{
			Type? baseType = type.BaseType;
			bool hasParentInSet = false;

			while (baseType != null &&
				   baseType != typeof(object) &&
				   baseType != typeof(ValueType) &&
				   !baseType.IsAssignableTo(typeof(Node)))
			{
				if (classMap.ContainsKey(baseType))
				{
					hasParentInSet = true;
					break;
				}
				baseType = baseType.BaseType;
			}

			if (!hasParentInSet)
			{
				roots.Add(type);
			}
		}

		// Process all roots
		foreach (Type root in roots)
		{
			AddTypeAndChildren(root);
		}

		return result;
	}

	public static void GenerateRefFile()
	{
		string docData = JsonSerializer.Serialize(GenerateReferences(), APIRefGenerationContext.Default.APIReferenceRoot);
		using FileAccess file = FileAccess.Open("res://def.json", FileAccess.ModeFlags.Write);
		file.StoreString(docData);
		file.Close();
	}

	private static string ProcessClassName(Type type)
	{
		if (type.IsAssignableTo(typeof(IScriptGDObject)))
		{
			return type.Name.TrimPrefix("PT");
		}
		return type.Name;
	}

	private static string? ProcessTypeName(Type? type)
	{
		if (type == null) return "nil";
		if (Nullable.GetUnderlyingType(type) is Type underlying)
			type = underlying;

		if (type == typeof(byte) ||
			type == typeof(sbyte) ||
			type == typeof(short) ||
			type == typeof(ushort) ||
			type == typeof(int) ||
			type == typeof(uint) ||
			type == typeof(long) ||
			type == typeof(ulong) ||
			type == typeof(float) ||
			type == typeof(double) ||
			type == typeof(decimal))
		{
			return "number";
		}

		if (type.IsAssignableTo(typeof(IScriptGDObject)))
		{
			return ProcessClassName(type);
		}

		if (type == typeof(Task))
		{
			return "nil";
		}
		else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
		{
			return ProcessTypeName(type.GetGenericArguments()[0]);
		}

		if (type == typeof(string))
		{
			return "string";
		}

		if (type == typeof(byte[]))
		{
			return "buffer";
		}

		if (type == typeof(bool))
		{
			return "boolean";
		}

		if (type == typeof(Nullable) || type == typeof(void))
		{
			return "nil";
		}

		if (type == typeof(object[]))
		{
			return "any";
		}

		if (type.IsAssignableTo(typeof(IDictionary)))
		{
			return "table";
		}

		if (type.IsArray)
		{
			string elementTypeName = ProcessTypeName(type.GetElementType()) ?? "nil";
			return "{ " + elementTypeName + " }";
		}

		if (type == typeof(PTCallback))
		{
			return "() -> ()";
		}

		if (type == typeof(PTFunction))
		{
			return "() -> ()";
		}

		// --- Proxies --- //

		if (type == typeof(Aabb))
		{
			return "Bounds";
		}

		// -------------- //

		if (type == typeof(object))
		{
			return "any";
		}

		if (type == typeof(ValueType))
		{
			return null;
		}

		if (type.IsEnum)
		{
			// Find the Enum's external name
			string name = ScriptService.EnumMap.FirstOrDefault(x => x.Value == type).Key;
			if (!string.IsNullOrEmpty(name))
				return name;
		}

		return type.Name;
	}

	public struct ScriptParameter
	{
		public string Name;
		public string? Type;
		public string? Description;
		public bool IsOptional;
		public string? DefaultValue;
	}

	public struct ScriptMethod
	{
		public string Name;
		public string? ReturnType;
		public string? Returns;
		public string? Description;
		public string? Remarks;
		public List<string>? Examples;
		public List<string>? SeeAlso;
		public List<ScriptParameter> Parameters;
		public bool IsAsync;
		public bool IsObsolete;
		public bool IsStatic;
		public bool IsSemiStatic;
	}

	public struct ScriptProperty
	{
		public string Name;
		public string? Type;
		public string? Description;
		public string? Remarks;
		public List<string>? SeeAlso;
		public bool IsAccessibleByScripts;
		public bool IsReadOnly;
		public bool IsObsolete;
		public bool IsStatic;
	}

	public struct ScriptEvent
	{
		public string Name;
		public string? Description;
		public List<ScriptParameter> Parameters;
	}

	public struct ScriptEnumValue
	{
		public string Name;
		public string? Description;
	}

	public struct ScriptEnum
	{
		public string Name;
		public string InternalName;
		public string? Description;
		public List<ScriptEnumValue> Options;
	}

	public struct ScriptClass
	{
		public string Name;
		public string? BaseType;
		public string? Category;
		public string? Description;
		public string? Remarks;
		public List<string>? Examples;
		public List<string>? SeeAlso;
		public List<ScriptProperty> Properties;
		public List<ScriptMethod> Methods;
		public List<ScriptEvent> Events;
		public bool IsStatic;
		public bool IsAbstract;
		public bool IsInstantiable;
		public string? StaticAlias;
	}

	public struct APIReferenceRoot
	{
		public string Version;
		public List<ScriptClass> Classes;
		public List<ScriptEnum> Enums;
		public List<string> InstanceClasses;
	}
}

internal sealed class XmlDocReader
{
	private readonly Dictionary<string, XElement> _members = new(StringComparer.Ordinal);

	public XmlDocReader(string xmlContent)
	{
		try
		{
			var doc = XDocument.Parse(xmlContent);
			foreach (XElement member in doc.Descendants("member"))
			{
				string? name = (string?)member.Attribute("name");
				if (name != null) _members[name] = member;
			}
		}
		catch { }
	}

	public string? GetMemberSummary(string key) => GetMemberSection(key, "summary");

	public Dictionary<string, string> GetMemberParams(string key)
	{
		Dictionary<string, string> result = [];
		if (!_members.TryGetValue(key, out XElement? el)) return result;
		foreach (XElement p in el.Elements("param"))
		{
			string? pName = (string?)p.Attribute("name");
			if (pName != null) result[pName] = RenderInline(p);
		}
		return result;
	}

	public string? GetMemberSection(string key, string tag)
	{
		if (!_members.TryGetValue(key, out XElement? el)) return null;
		XElement? section = el.Element(tag);
		return section != null ? RenderInline(section) : null;
	}

	public List<string>? GetMemberExamples(string key)
	{
		if (!_members.TryGetValue(key, out XElement? el)) return null;
		List<string> list = el.Elements("example").Select(RenderInline).ToList();
		return list.Count > 0 ? list : null;
	}

	public List<string>? GetMemberSeeAlso(string key)
	{
		if (!_members.TryGetValue(key, out XElement? el)) return null;
		List<string> list = el.Elements("seealso")
			.Select(e => ((string?)e.Attribute("cref") ?? "").Split('.').Last())
			.Where(s => s.Length > 0)
			.ToList();
		return list.Count > 0 ? list : null;
	}

	public (string? Summary, string? Remarks, string? Returns, List<string>? Examples, List<string>? SeeAlso, Dictionary<string, string> ParamDocs)
		GetMethodDocs(string typeFullName, string methodName)
	{
		string prefix = "M:" + typeFullName + "." + methodName;
		string? foundKey = null;
		foreach (string k in _members.Keys)
		{
			if (k == prefix || (k.StartsWith(prefix) && k.Length > prefix.Length && k[prefix.Length] == '('))
			{
				foundKey = k;
				break;
			}
		}
		if (foundKey == null) return (null, null, null, null, null, []);

		Dictionary<string, string> paramDocs = [];
		if (_members.TryGetValue(foundKey, out XElement? el))
		{
			foreach (XElement p in el.Elements("param"))
			{
				string? pName = (string?)p.Attribute("name");
				if (pName != null) paramDocs[pName] = RenderInline(p);
			}
		}

		return (
			GetMemberSection(foundKey, "summary"),
			GetMemberSection(foundKey, "remarks"),
			GetMemberSection(foundKey, "returns"),
			GetMemberExamples(foundKey),
			GetMemberSeeAlso(foundKey),
			paramDocs
		);
	}

	private static string RenderInline(XElement el)
	{
		var sb = new System.Text.StringBuilder();
		foreach (XNode node in el.Nodes())
		{
			if (node is XText text)
			{
				string[] parts = text.Value.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length > 0) { sb.Append(' '); sb.Append(string.Join(" ", parts)); }
			}
			else if (node is XElement child)
			{
				switch (child.Name.LocalName)
				{
					case "c": sb.Append(" `").Append(child.Value.Trim()).Append('`'); break;
					case "code": sb.Append("\n```lua\n").Append(child.Value.Trim()).Append("\n```"); break;
					case "para": sb.Append("\n\n").Append(RenderInline(child).TrimStart()); break;
					case "see": sb.Append(' ').Append(((string?)child.Attribute("cref") ?? "").Split('.').Last()); break;
					default: sb.Append(RenderInline(child)); break;
				}
			}
		}
		return sb.ToString().Trim();
	}
}

[JsonSourceGenerationOptions(IncludeFields = true)]
[JsonSerializable(typeof(APIReferenceRoot))]
[JsonSerializable(typeof(ScriptClass))]
[JsonSerializable(typeof(ScriptEnum))]
[JsonSerializable(typeof(ScriptEnumValue))]
[JsonSerializable(typeof(ScriptEvent))]
[JsonSerializable(typeof(ScriptProperty))]
[JsonSerializable(typeof(ScriptMethod))]
[JsonSerializable(typeof(ScriptParameter))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(float))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(List<ScriptClass>))]
[JsonSerializable(typeof(List<ScriptEnum>))]
[JsonSerializable(typeof(List<ScriptEnumValue>))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(List<ScriptProperty>))]
[JsonSerializable(typeof(List<ScriptMethod>))]
[JsonSerializable(typeof(List<ScriptEvent>))]
[JsonSerializable(typeof(List<ScriptParameter>))]
internal partial class APIRefGenerationContext : JsonSerializerContext { }
