// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'obavijest.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Obavijest _$ObavijestFromJson(Map<String, dynamic> json) => Obavijest(
      id: (json['id'] as num?)?.toInt(),
      naslov: json['naslov'] as String?,
      tekst: json['tekst'] as String?,
      slikaBase64: json['slikaBase64'] as String?,
      isActive: json['isActive'] as bool?,
      createdAt: json['createdAt'] == null
          ? null
          : DateTime.parse(json['createdAt'] as String),
    );

Map<String, dynamic> _$ObavijestToJson(Obavijest instance) =>
    <String, dynamic>{
      'id': instance.id,
      'naslov': instance.naslov,
      'tekst': instance.tekst,
      'slikaBase64': instance.slikaBase64,
      'isActive': instance.isActive,
      'createdAt': instance.createdAt?.toIso8601String(),
    };
