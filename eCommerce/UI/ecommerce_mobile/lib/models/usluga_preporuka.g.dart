// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'usluga_preporuka.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

UslugaPreporuka _$UslugaPreporukaFromJson(Map<String, dynamic> json) =>
    UslugaPreporuka(
      usluga: json['usluga'] == null
          ? null
          : Usluga.fromJson(json['usluga'] as Map<String, dynamic>),
      skor: (json['skor'] as num?)?.toDouble(),
      contentBasedSkor: (json['contentBasedSkor'] as num?)?.toDouble(),
      popularityBasedSkor: (json['popularityBasedSkor'] as num?)?.toDouble(),
      objasnjenje: json['objasnjenje'] as String?,
    );

Map<String, dynamic> _$UslugaPreporukaToJson(UslugaPreporuka instance) =>
    <String, dynamic>{
      'usluga': instance.usluga,
      'skor': instance.skor,
      'contentBasedSkor': instance.contentBasedSkor,
      'popularityBasedSkor': instance.popularityBasedSkor,
      'objasnjenje': instance.objasnjenje,
    };
